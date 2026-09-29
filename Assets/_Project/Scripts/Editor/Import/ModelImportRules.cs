using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SailorMoon.EditorTools.Import
{
    /// <summary>
    /// Cómo se importa cada cosa según la carpeta donde cae. Nadie ajusta un
    /// importador a mano: si un modelo se ve mal, se arregla AQUÍ y vale para
    /// todos los que vengan detrás.
    ///
    ///   Art/Characters/*   personaje: Humanoid, sin materiales (el prefab lleva el suyo)
    ///   Art/Animations/Humanoid/*   clips de Mixamo: Humanoid, In Place de verdad
    ///   Art/Props/*        decorado: sin esqueleto, UV de lightmap, material toon
    ///
    /// La versión de las reglas va en <see cref="Version"/>: subirla fuerza a
    /// Unity a reimportar todo lo afectado. Sin eso, un cambio aquí no se nota
    /// hasta que alguien reimporta a mano (el mismo fallo que la trampa 41 de
    /// Godot, que no reimportaba un .glb regenerado).
    /// </summary>
    class ModelImportRules : AssetPostprocessor
    {
        const uint Version = 6;
        const string Characters = "Assets/_Project/Art/Characters/";
        const string Animations = "Assets/_Project/Art/Animations/Humanoid/";
        const string Props = "Assets/_Project/Art/Props/";
        const string ToonShader = "SailorMoon/Toon";

        /// Lo que tiene que poder durar indefinidamente. Un gesto que empieza y
        /// acaba (un golpe, asentir) puesto en bucle se ve como un tic.
        static readonly HashSet<string> Loops = new()
        {
            "idle", "walk", "run", "jump_loop", "ledge_grab", "dizzy", "talk_idle", "thinking",
        };

        /// <summary>
        /// Tramo de cada clip que se usa, en segundos del archivo original.
        ///
        /// Algunos clips de Mixamo hacen más de lo que dice su nombre, y se
        /// midió con la altura del cuerpo a lo largo del clip (RootT.y):
        /// · jump_start («Jump») es un salto ENTERO: sube hasta 0,31 s,
        ///   aterriza hacia 0,62 s y se incorpora. Del salto solo se usa el
        ///   impulso; la caída la hace jump_loop y el aterrizaje, land.
        /// · land («Falling To Landing») EMPIEZA EN EL AIRE: el cuerpo va 0,7 m
        ///   por encima de lo normal y toca suelo hacia 0,27 s. Entrando desde
        ///   el principio, al aterrizar se veía otro salto.
        /// </summary>
        static readonly Dictionary<string, (float from, float to)> Ranges = new()
        {
            ["jump_start"] = (0.08f, 0.33f),
            ["land"] = (0.27f, 1.07f),
        };

        public override uint GetVersion() => Version;

        void OnPreprocessModel()
        {
            var importer = (ModelImporter)assetImporter;
            importer.importCameras = false;
            importer.importLights = false;
            // La conversión de ejes SOLO para lo que sale de Blender (los .glb
            // convertidos). Los FBX de Mixamo ya vienen con los ejes de Unity
            // (Y arriba, +Z adelante); convertirlos otra vez dejaba a Serena
            // corriendo de espaldas (docs/TRAMPAS-UNITY.md, U9).
            importer.bakeAxisConversion = !IsFromMixamo();
            importer.isReadable = false;

            if (assetPath.StartsWith(Characters))
            {
                bool mixamo = IsMixamoCharacter();
                importer.animationType = mixamo ? ModelImporterAnimationType.Human : ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                // Los personajes de Mixamo no traen animación propia; los .glb
                // convertidos (el guardián, Luna) sí.
                importer.importAnimation = !mixamo;
                // El material lo pone el prefab (editable, con el toon). El que
                // trae el FBX es un PBR que no se usa.
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                // Los huesos del pelo tienen que existir como Transform para la
                // física de coletas: nada de optimizar la jerarquía.
                importer.optimizeGameObjects = false;
                importer.importBlendShapes = false;
            }
            else if (assetPath.StartsWith(Animations))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                // Cada clip crea su propio avatar desde su esqueleto, y
                // HumanoidTPose le pone la referencia en pose T. (Probado y
                // descartado: usar el avatar de Serena para todos. Los FBX «sin
                // piel» de Mixamo traen otro esqueleto y Unity avisa de 19 cm de
                // desfase en la cadera; ver TRAMPAS-UNITY U10.)
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            }
            else if (assetPath.StartsWith(Props))
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                // UV2 para los lightmaps: sin ellas el decorado no se puede hornear.
                importer.generateSecondaryUV = true;
                importer.importBlendShapes = false;
                // Cada material que se llame como una superficie compartida
                // («madera», «piedra»…) se enlaza a ella en vez de importarse.
                foreach (var (name, mat) in SurfaceLibrary.All())
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), mat);
            }
        }

        bool IsFromMixamo() => assetPath.StartsWith(Animations) ||
                                (assetPath.StartsWith(Characters) && IsMixamoCharacter());

        bool IsMixamoCharacter()
        {
            // Los rigueados de Mixamo llegan tal cual (.fbx con nombre de
            // carpeta). Los convertidos desde .glb por Blender, también en
            // .fbx, pero sin huesos mixamorig: se distinguen por la lista del
            // importador (tools/importar_desde_godot.py).
            string name = Path.GetFileNameWithoutExtension(assetPath);
            return name.StartsWith("Sailor") || name is "Serena" or "ChibiMoon" or "TuxedoMask";
        }

        /// <summary>
        /// Los clips de Mixamo, arreglados de una vez:
        ///
        /// · Se llaman como el archivo, no «mixamo.com».
        /// · La altura de la raíz NO se hornea en la pose y se mide desde los
        ///   pies. Con la raíz sin aplicar (Animator.applyRootMotion = false),
        ///   lo que la animación suba el cuerpo entero se descarta: el salto lo
        ///   pone la física. En Godot la animación y la física se sumaban y en
        ///   el doble salto Serena subía de más y al aterrizar atravesaba el
        ///   suelo (trampa 85). Aquí eso no puede pasar por construcción, y el
        ///   agacharse al aterrizar se conserva porque es relativo a los pies.
        /// · El giro y el avance horizontal se hornean: van «In Place».
        /// </summary>
        void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(Animations)) return;
            var importer = (ModelImporter)assetImporter;
            string name = Path.GetFileNameWithoutExtension(assetPath);

            var clips = importer.defaultClipAnimations;
            float fps = importer.importedTakeInfos.Length > 0 ? importer.importedTakeInfos[0].sampleRate : 30f;
            for (int i = 0; i < clips.Length; i++)
            {
                var c = clips[i];
                c.name = name;
                if (Ranges.TryGetValue(name, out var range))
                {
                    c.firstFrame = range.from * fps;
                    c.lastFrame = Mathf.Min(range.to * fps, c.lastFrame);
                }
                c.loopTime = Loops.Contains(name);
                c.loopPose = c.loopTime;
                c.lockRootRotation = true;
                c.keepOriginalOrientation = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalPositionXZ = true;
                c.lockRootHeightY = false;
                c.heightFromFeet = true;
                clips[i] = c;
            }
            importer.clipAnimations = clips;
        }

        /// <summary>
        /// Los materiales de los props salen ya en toon, con su textura. Sin
        /// contorno: en el decorado es un draw call más por pieza y el estilo
        /// lo lleva en los personajes.
        /// </summary>
        void OnPreprocessMaterialDescription(MaterialDescription description, Material material,
            AnimationClip[] animations)
        {
            if (!assetPath.StartsWith(Props)) return;
            var shader = Shader.Find(ToonShader);
            if (shader == null)
            {
                Debug.LogWarning($"[Import] No encuentro {ToonShader}; {assetPath} se queda con el material por defecto.");
                return;
            }
            material.shader = shader;
            if (description.TryGetProperty("DiffuseColor", out TexturePropertyDescription tex) && tex.texture != null)
            {
                material.SetTexture("_BaseMap", tex.texture);
                material.SetTextureScale("_BaseMap", tex.scale);
                material.SetTextureOffset("_BaseMap", tex.offset);
            }
            else if (description.TryGetProperty("DiffuseColor", out Vector4 color))
            {
                material.SetColor("_BaseColor", color);
            }
            material.SetFloat("_OutlineWidth", 0f);
            material.SetShaderPassEnabled("SRPDefaultUnlit", false);
        }

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = false;
            if (assetPath.StartsWith(Characters))
                importer.maxTextureSize = 2048;
            else if (assetPath.StartsWith(Props))
                importer.maxTextureSize = 1024;
        }

        /// Música en streaming (no ocupa memoria entera); efectos cortos
        /// descomprimidos al cargar para que suenen sin latencia.
        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/_Project/Audio/")) return;
            var importer = (AudioImporter)assetImporter;
            var s = importer.defaultSampleSettings;
            bool music = assetPath.Contains("/Music/") || assetPath.Contains("/Ambience/");
            s.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = music ? 0.6f : 0.8f;
            importer.defaultSampleSettings = s;
            importer.forceToMono = !music;
        }
    }
}
