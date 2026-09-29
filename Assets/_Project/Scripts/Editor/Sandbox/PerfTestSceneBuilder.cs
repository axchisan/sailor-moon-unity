using System.Collections.Generic;
using System.IO;
using System.Linq;
using SailorMoon.CameraRig;
using SailorMoon.Diagnostics;
using SailorMoon.EditorTools.Characters;
using SailorMoon.EditorTools.Terrain;
using SailorMoon.Player;
using SailorMoon.UI;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SailorMoon.EditorTools.Sandbox
{
    /// <summary>
    /// Monta la escena de la prueba de rendimiento: un parque del tamaño del
    /// Nivel 1 de Godot, con la misma densidad de decorado, para medir en el
    /// mismo teléfono y comparar.
    ///
    /// OJO: esto es un ANDAMIO, no el sistema de niveles. Genera la escena
    /// UNA vez y la guarda; a partir de ahí es una escena normal que se
    /// esculpe, se pinta y se decora a mano en el editor. El juego no genera
    /// nada al arrancar — ese fue el error de fondo en Godot, que montaba el
    /// parque entero en cada partida (docs/aprendido/LECCIONES.md §1).
    /// </summary>
    static class PerfTestSceneBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Sandbox/PerfTest.unity";
        const string SceneFolder = "Assets/_Project/Scenes/Sandbox/PerfTest";
        const string SurfaceFolder = "Assets/_Project/Art/Terrain/Surfaces";
        const string PropPrefabs = "Assets/_Project/Prefabs/Props";
        const string PropArt = "Assets/_Project/Art/Props";

        const float TerrainSize = 240f;
        const float TerrainHeight = 40f;
        const float PathRadius = 34f;

        [MenuItem("Sailor Moon/Sandbox/Crear escena de prueba de rendimiento")]
        static void Build()
        {
            // Sin diálogos: un «¿guardar cambios?» modal bloquea el editor y,
            // con él, a cualquier agente que lo esté usando por el MCP. Si la
            // escena abierta tiene archivo y cambios, se guarda; si no, se
            // descarta (una escena sin título no tiene nada que perder).
            var current = EditorSceneManager.GetActiveScene();
            if (current.isDirty && !string.IsNullOrEmpty(current.path)) EditorSceneManager.SaveScene(current);
            Directory.CreateDirectory(SceneFolder);

            var layers = BuildSurfaces();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = BuildLighting();
            var terrain = BuildTerrain(layers);
            var decor = BuildDecor(terrain);
            BuildLightProbes(terrain);

            var serena = PlacePlayer(terrain);
            var (brain, vcam) = BuildCamera(serena);
            var canvas = BuildUI(serena);
            BuildBenchmark(terrain, decor, sun, serena, brain, vcam, canvas);

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[PerfTest] Escena montada. Falta hornear la luz: Sailor Moon ▸ Sandbox ▸ Hornear luz.");
        }

        [MenuItem("Sailor Moon/Sandbox/Hornear luz")]
        static void Bake()
        {
            Lightmapping.BakeAsync();
        }

        // ------------------------------------------------------------------
        // Superficies: las recetas de paleta_terreno.gd, con el ajuste de
        // color «vivo» (contraste 1,08, saturación 1,25) ya aplicado. En
        // Godot se horneaba solo en el móvil; aquí hay un único aspecto.

        static TerrainLayer[] BuildSurfaces()
        {
            Directory.CreateDirectory(SurfaceFolder);
            return new[]
            {
                Recipe("Grass", new(0.43f, 0.58f, 0.36f), new(0.33f, 0.47f, 0.30f), new(0.56f, 0.70f, 0.42f), 3.4f, 3, 0.62f, 0.20f, 0f),
                Recipe("GrassFar", new(0.38f, 0.52f, 0.34f), new(0.31f, 0.44f, 0.29f), new(0.47f, 0.60f, 0.38f), 5.5f, 3, 0.5f, 0.12f, 0f),
                Recipe("Path", new(0.76f, 0.70f, 0.54f), new(0.66f, 0.59f, 0.44f), new(0.84f, 0.79f, 0.63f), 2.2f, 3, 0.45f, 0.26f, 0.22f),
                Recipe("Rock", new(0.62f, 0.62f, 0.60f), new(0.50f, 0.50f, 0.50f), new(0.74f, 0.74f, 0.71f), 1.8f, 4, 0.55f, 0.18f, 0f),
            };
        }

        static TerrainLayer Recipe(string name, Color b, Color d, Color l, float scale, int tones, float strength, float grain, float streaks)
        {
            string path = $"{SurfaceFolder}/{name}.asset";
            var r = AssetDatabase.LoadAssetAtPath<TerrainSurfaceRecipe>(path);
            if (r == null)
            {
                r = ScriptableObject.CreateInstance<TerrainSurfaceRecipe>();
                r.baseColor = Vivid(b); r.darkColor = Vivid(d); r.lightColor = Vivid(l);
                r.scale = scale; r.tones = tones; r.strength = strength; r.grain = grain; r.streaks = streaks;
                AssetDatabase.CreateAsset(r, path);
            }
            return r.Bake();
        }

        static Color Vivid(Color c)
        {
            var v = Vector3.LerpUnclamped(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(c.r, c.g, c.b), 1.08f);
            float lum = Vector3.Dot(v, new Vector3(0.2125f, 0.7154f, 0.0721f));
            v = Vector3.LerpUnclamped(new Vector3(lum, lum, lum), v, 1.25f);
            return new Color(Mathf.Max(v.x, 0), Mathf.Max(v.y, 0), Mathf.Max(v.z, 0));
        }

        // ------------------------------------------------------------------

        static Light BuildLighting()
        {
            var sunGo = new GameObject("Sol");
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            sun.lightmapBakeType = LightmapBakeType.Mixed;
            RenderSettings.sun = sun;

            var skyMat = new Material(Shader.Find("SailorMoon/SkyGradient"));
            AssetDatabase.CreateAsset(skyMat, $"{SceneFolder}/Sky.mat");
            RenderSettings.skybox = skyMat;

            // Ambiente de tres colores: se evalúa como una sonda esférica, por
            // vértice. Nada de muestrear el cielo por píxel (en Godot, las dos
            // muestras de cubemap del terreno costaban un 25 %).
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.90f);
            RenderSettings.ambientEquatorColor = new Color(0.64f, 0.66f, 0.70f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.44f, 0.38f);
            RenderSettings.subtractiveShadowColor = new Color(0.55f, 0.52f, 0.68f);

            // Sin reflejos de entorno: el cel shading no los usa y cada uno es
            // una muestra de cubemap por píxel.
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;
            RenderSettings.reflectionIntensity = 0f;

            // Perspectiva aérea: la niebla de siempre, que en URP se calcula
            // por vértice. UNA sola (en Godot había dos velos sumados, trampa 112).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 220f;
            RenderSettings.fogColor = new Color(0.80f, 0.86f, 0.98f);

            var ls = new LightingSettings
            {
                bakedGI = true,
                realtimeGI = false,
                lightmapper = LightingSettings.Lightmapper.ProgressiveGPU,
                // Subtractive: todo lo estático lleva la luz y sus sombras
                // horneadas; en tiempo real solo se calcula la sombra de lo que
                // se mueve. Es el modo más barato que existe en móvil.
                mixedBakeMode = MixedLightingMode.Subtractive,
                directionalityMode = LightmapsMode.NonDirectional,
                lightmapResolution = 3f,
                lightmapMaxSize = 1024,
                lightmapPadding = 2,
                directSampleCount = 32,
                indirectSampleCount = 128,
                environmentSampleCount = 64,
                maxBounces = 1,
                filteringMode = LightingSettings.FilterMode.Auto,
                lightmapCompression = LightmapCompression.NormalQuality,
            };
            AssetDatabase.CreateAsset(ls, $"{SceneFolder}/Lighting.lighting");
            Lightmapping.lightingSettings = ls;
            return sun;
        }

        // ------------------------------------------------------------------

        static UnityEngine.Terrain BuildTerrain(TerrainLayer[] layers)
        {
            var data = new TerrainData
            {
                heightmapResolution = 257,
                alphamapResolution = 256,
                baseMapResolution = 512,
            };
            data.size = new Vector3(TerrainSize, TerrainHeight, TerrainSize);
            // El asset PRIMERO: las texturas de mezcla de capas son sub-assets
            // suyos, y si se pintan antes de que exista no se guardan (sale
            // todo con la primera capa, sin error).
            string dataPath = $"{SceneFolder}/TerrainData_Park.asset";
            AssetDatabase.DeleteAsset(dataPath);
            AssetDatabase.CreateAsset(data, dataPath);
            data.terrainLayers = layers;

            int hr = data.heightmapResolution;
            var h = new float[hr, hr];
            for (int y = 0; y < hr; y++)
            for (int x = 0; x < hr; x++)
            {
                Vector2 p = new Vector2(x, y) / (hr - 1) * TerrainSize - Vector2.one * TerrainSize / 2f;
                h[y, x] = Height(p) / TerrainHeight;
            }
            data.SetHeights(0, 0, h);

            int ar = data.alphamapResolution;
            var a = new float[ar, ar, layers.Length];
            for (int y = 0; y < ar; y++)
            for (int x = 0; x < ar; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f) / ar * TerrainSize - Vector2.one * TerrainSize / 2f;
                float d = p.magnitude;
                float steep = data.GetSteepness((x + 0.5f) / ar, (y + 0.5f) / ar);
                float path = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2.2f, 3.0f, PathDistance(p)));
                float rock = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(36f, 46f, steep));
                float far = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(62f, 80f, d));
                float grass = 1f;
                // Orden de prioridad: roca > camino > hierba lejana > hierba.
                float wRock = rock;
                float wPath = path * (1 - wRock);
                float wFar = far * (1 - wRock - wPath);
                float wGrass = grass * (1 - wRock - wPath - wFar);
                a[y, x, 0] = wGrass; a[y, x, 1] = wFar; a[y, x, 2] = wPath; a[y, x, 3] = wRock;
            }
            data.SetAlphamaps(0, 0, a);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            var go = UnityEngine.Terrain.CreateTerrainGameObject(data);
            go.name = "Terreno";
            go.transform.position = new Vector3(-TerrainSize / 2f, 0, -TerrainSize / 2f);
            var t = go.GetComponent<UnityEngine.Terrain>();
            t.drawInstanced = true;
            t.heightmapPixelError = 8;
            t.basemapDistance = 90;
            // El terreno no proyecta sombra en tiempo real: su relieve ya va
            // horneado y el pase de sombra de una malla de este tamaño es caro.
            t.shadowCastingMode = ShadowCastingMode.Off;
            t.materialTemplate = GraphicsSettings.currentRenderPipeline.defaultTerrainMaterial;
            // SIN lightmap: en Unity 6000.6 el Terrain de URP con lightmap sale
            // en blanco y negro según las capas (docs/TRAMPAS-UNITY.md, U5).
            // Se ilumina en tiempo real y recibe el ambiente por sondas. Es
            // además lo que la prueba tiene que medir: cuánto cuesta iluminar
            // el suelo en la Adreno 619, que era el cuello de Godot.
            GameObjectUtility.SetStaticEditorFlags(go,
                StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            return t;
        }

        /// Llano en el centro con lomas suaves, y una cordillera que cierra el
        /// parque: el mismo esquema que terreno_nivel.gd, a mano alzada.
        static float Height(Vector2 p)
        {
            float d = p.magnitude;
            float hills = (Mathf.PerlinNoise(p.x * 0.018f + 11.3f, p.y * 0.018f + 7.1f) - 0.5f) * 3.2f;
            float flat = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(40f, 60f, d));
            float h = 2f + hills * (1f - flat * 0.8f);
            float ridge = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(82f, 116f, d));
            float peaks = Mathf.PerlinNoise(p.x * 0.035f + 3.7f, p.y * 0.035f + 1.9f);
            h += ridge * (12f + peaks * 22f);
            // El camino, plano: se aplana hacia la altura media.
            float onPath = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2.5f, 6f, PathDistance(p)));
            return Mathf.Lerp(h, 2f, onPath * 0.85f);
        }

        /// Un paseo en lazo alrededor del centro y un sendero que lo cruza.
        static float PathDistance(Vector2 p)
        {
            float ang = Mathf.Atan2(p.y, p.x);
            float r = PathRadius + Mathf.Sin(ang * 3f) * 5f;
            float loop = Mathf.Abs(p.magnitude - r);
            float cross = Mathf.Abs(p.y - Mathf.Sin(p.x * 0.05f) * 6f);
            if (Mathf.Abs(p.x) > 50f) cross = 999f;
            return Mathf.Min(loop, cross);
        }

        // ------------------------------------------------------------------
        // Decorado: ~1.600 props, la densidad del Nivel 1 de Godot (~1.860).

        struct PropSpec
        {
            public string name; public int count; public float minR, maxR;
            public float scaleMin, scaleMax; public float clearOfPath; public bool shadows;
            public PropSpec(string n, int c, float min, float max, float sMin = 0.9f, float sMax = 1.15f,
                float clear = 3.5f, bool sh = true)
            { name = n; count = c; minR = min; maxR = max; scaleMin = sMin; scaleMax = sMax; clearOfPath = clear; shadows = sh; }
        }

        static GameObject BuildDecor(UnityEngine.Terrain terrain)
        {
            var specs = new[]
            {
                new PropSpec("CherryTree", 200, 6, 75, 0.9f, 1.3f, 5f),
                new PropSpec("arbol_cerezo_lejos", 160, 80, 112, 1.2f, 1.8f, 0),
                new PropSpec("arbusto", 220, 4, 78),
                new PropSpec("seto_bajo", 80, 4, 70),
                new PropSpec("mata_hierba", 450, 3, 80, 0.8f, 1.3f, 2.8f, false),
                new PropSpec("macizo_flores", 90, 4, 60, 0.9f, 1.2f, 3f, false),
                new PropSpec("roca", 120, 3, 90, 0.7f, 1.5f),
                new PropSpec("roca_grande", 40, 10, 95, 0.8f, 1.6f, 6f),
                new PropSpec("tocon", 30, 5, 75),
                new PropSpec("piedrecitas", 100, 3, 70, 0.8f, 1.2f, 2.8f, false),
                new PropSpec("ramitas", 60, 3, 70, 0.8f, 1.2f, 2.8f, false),
                new PropSpec("hojarasca", 60, 3, 70, 0.8f, 1.2f, 2.8f, false),
            };

            var decor = new GameObject("Decorado");
            var rng = new System.Random(20260929);
            var occupied = new List<Vector3>();
            foreach (var spec in specs)
            {
                var prefab = PropPrefab(spec.name, spec.shadows);
                if (prefab == null) continue;
                var group = new GameObject(spec.name).transform;
                group.SetParent(decor.transform, false);
                int placed = 0;
                for (int attempt = 0; placed < spec.count && attempt < spec.count * 20; attempt++)
                {
                    float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float r = Mathf.Sqrt(Mathf.Lerp(spec.minR * spec.minR, spec.maxR * spec.maxR, (float)rng.NextDouble()));
                    var p2 = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
                    if (spec.clearOfPath > 0 && PathDistance(p2) < spec.clearOfPath) continue;
                    var pos = new Vector3(p2.x, 0, p2.y);
                    pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
                    // Sitio: los grandes no se pisan entre sí.
                    float gap = spec.shadows ? 1.6f : 0.6f;
                    if (occupied.Any(o => (o - pos).sqrMagnitude < gap * gap)) continue;
                    occupied.Add(pos);

                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group);
                    inst.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0));
                    inst.transform.localScale = Vector3.one * Mathf.Lerp(spec.scaleMin, spec.scaleMax, (float)rng.NextDouble());
                    placed++;
                }
            }

            // Lo que va junto al camino: faroles y bancos, mirando al paseo.
            AlongPath(decor.transform, terrain, PropPrefab("farol_piedra", true), 16, 3.4f, 0f);
            AlongPath(decor.transform, terrain, PropPrefab("banco_parque", true), 14, 3.6f, 0.5f);
            AlongPath(decor.transform, terrain, PropPrefab("papelera", true), 8, 3.2f, 0.25f);
            return decor;
        }

        static void AlongPath(Transform parent, UnityEngine.Terrain terrain, GameObject prefab, int count, float offset, float phase)
        {
            if (prefab == null) return;
            var group = new GameObject(prefab.name).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < count; i++)
            {
                float ang = (i + phase) / count * Mathf.PI * 2f;
                float r = PathRadius + Mathf.Sin(ang * 3f) * 5f + ((i % 2 == 0) ? offset : -offset);
                var p = new Vector3(Mathf.Cos(ang) * r, 0, Mathf.Sin(ang) * r);
                p.y = terrain.SampleHeight(p);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group);
                var toPath = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * ((i % 2 == 0) ? -1 : 1);
                inst.transform.SetPositionAndRotation(p, Quaternion.LookRotation(toPath));
            }
        }

        /// <summary>
        /// Prefab de un prop: el modelo, marcado estático (lightmap, agrupado,
        /// oclusión) y con un colisionador aproximado si es grande. Es el
        /// prefab que luego se arrastra a mano al decorar.
        /// </summary>
        static GameObject PropPrefab(string name, bool shadows)
        {
            Directory.CreateDirectory(PropPrefabs);
            string path = $"{PropPrefabs}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject root;
            if (name == "CherryTree")
            {
                // Un cerezo con dos niveles de detalle: el de cerca y el de
                // lejos del kit. Unity cambia de uno a otro según el tamaño en
                // pantalla.
                root = new GameObject(name);
                var near = Model("arbol_cerezo", root.transform);
                var far = Model("arbol_cerezo_lejos", root.transform);
                var lod = root.AddComponent<LODGroup>();
                lod.SetLODs(new[]
                {
                    new LOD(0.18f, near.GetComponentsInChildren<Renderer>()),
                    new LOD(0.02f, far.GetComponentsInChildren<Renderer>()),
                });
                lod.RecalculateBounds();
            }
            else
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{PropArt}/{name}/{name}.fbx");
                if (model == null) { Debug.LogWarning($"[PerfTest] Falta el modelo {name}"); return null; }
                root = new GameObject(name);
                Model(name, root.transform);
            }

            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                r.receiveShadows = true;
            }
            var flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.OccludeeStatic;
            foreach (var t in root.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);

            // Colisión: solo lo que tiene bulto. La hierba y las piedrecitas
            // se atraviesan.
            if (shadows && renderers.Length > 0)
            {
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                if (name.Contains("cerezo") || name == "CherryTree" || name == "tocon")
                {
                    var c = root.AddComponent<CapsuleCollider>();
                    c.radius = name == "tocon" ? 0.35f : 0.3f;
                    c.height = Mathf.Min(b.size.y, 3f);
                    c.center = new Vector3(0, c.height / 2f, 0);
                }
                else
                {
                    var c = root.AddComponent<BoxCollider>();
                    c.center = b.center - root.transform.position;
                    c.size = b.size;
                }
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject Model(string name, Transform parent)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{PropArt}/{name}/{name}.fbx");
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            return inst;
        }

        static void BuildLightProbes(UnityEngine.Terrain terrain)
        {
            // Para lo que se mueve (Serena, los enemigos): la luz horneada del
            // entorno le llega por estas sondas. Una rejilla cada 12 m sobre
            // la zona jugable, a dos alturas.
            var go = new GameObject("Sondas de luz");
            var group = go.AddComponent<LightProbeGroup>();
            var pts = new List<Vector3>();
            for (float x = -72; x <= 72; x += 12)
            for (float z = -72; z <= 72; z += 12)
            {
                var p = new Vector3(x, 0, z);
                float y = terrain.SampleHeight(p);
                pts.Add(new Vector3(x, y + 1.2f, z));
                pts.Add(new Vector3(x, y + 5f, z));
            }
            group.probePositions = pts.ToArray();
        }

        // ------------------------------------------------------------------

        static GameObject PlacePlayer(UnityEngine.Terrain terrain)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Serena.prefab")
                         ?? CharacterBuilder.BuildSerena();
            var serena = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var spawn = new Vector3(PathRadius, 0, 0);
            spawn.y = terrain.SampleHeight(spawn) + 0.1f;
            serena.transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(Vector3.forward));
            return serena;
        }

        static (CinemachineBrain, CinemachineCamera) BuildCamera(GameObject serena)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 400f;
            cam.clearFlags = CameraClearFlags.Skybox;
            var brain = camGo.AddComponent<CinemachineBrain>();

            var target = serena.transform.Find("CameraTarget");
            var vGo = new GameObject("Camara jugadora");
            var vcam = vGo.AddComponent<CinemachineCamera>();
            vcam.Follow = target;
            vcam.LookAt = target;
            vcam.Lens.FieldOfView = 55f;
            var orbit = vGo.AddComponent<CinemachineOrbitalFollow>();
            orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbit.Radius = 5.5f;
            orbit.HorizontalAxis.Wrap = true;
            orbit.VerticalAxis.Range = new Vector2(-10f, 55f);
            orbit.VerticalAxis.Value = 16f;
            orbit.VerticalAxis.Center = 16f;
            vGo.AddComponent<CinemachineRotationComposer>();
            var deocc = vGo.AddComponent<CinemachineDeoccluder>();
            deocc.CollideAgainst = LayerMask.GetMask("Default");
            var look = vGo.AddComponent<CameraLook>();
            var so = new SerializedObject(look);
            so.FindProperty("_input").objectReferenceValue = serena.GetComponent<InputReader>();
            so.ApplyModifiedPropertiesWithoutUndo();

            var motor = new SerializedObject(serena.GetComponent<PlayerMotor>());
            motor.FindProperty("_cameraTransform").objectReferenceValue = camGo.transform;
            motor.ApplyModifiedPropertiesWithoutUndo();
            return (brain, vcam);
        }

        static Canvas BuildUI(GameObject serena)
        {
            var canvasGo = new GameObject("Interfaz");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 1f;

            var circle = CircleSprite();
            var touchGo = new GameObject("Controles tactiles", typeof(RectTransform));
            touchGo.transform.SetParent(canvasGo.transform, false);
            Stretch(touchGo.GetComponent<RectTransform>());
            var stickBase = Img("Joystick", touchGo.transform, circle, new Color(1, 1, 1, 0.25f), 220);
            var knob = Img("Palanca", stickBase.transform, circle, new Color(1, 1, 1, 0.6f), 96);
            var jump = Img("Saltar", touchGo.transform, circle, new Color(1f, 0.8f, 0.9f, 0.55f), 150);
            Anchor(jump, new Vector2(1, 0), new Vector2(-130, 130));
            var attack = Img("Atacar", touchGo.transform, circle, new Color(1f, 0.55f, 0.75f, 0.55f), 190);
            Anchor(attack, new Vector2(1, 0), new Vector2(-300, 110));

            var touch = touchGo.AddComponent<TouchControls>();
            var so = new SerializedObject(touch);
            so.FindProperty("_input").objectReferenceValue = serena.GetComponent<InputReader>();
            so.FindProperty("_stickBase").objectReferenceValue = stickBase;
            so.FindProperty("_stickKnob").objectReferenceValue = knob;
            so.FindProperty("_jumpButton").objectReferenceValue = jump;
            so.FindProperty("_attackButton").objectReferenceValue = attack;
            so.ApplyModifiedPropertiesWithoutUndo();

            var textGo = new GameObject("Rendimiento", typeof(RectTransform));
            textGo.transform.SetParent(canvasGo.transform, false);
            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(16, -12);
            rt.sizeDelta = new Vector2(900, 140);
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.raycastTarget = false;
            textGo.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.8f);
            var overlay = canvasGo.AddComponent<PerfOverlay>();
            var oso = new SerializedObject(overlay);
            oso.FindProperty("_label").objectReferenceValue = text;
            oso.ApplyModifiedPropertiesWithoutUndo();
            return canvas;
        }

        static void BuildBenchmark(UnityEngine.Terrain terrain, GameObject decor, Light sun, GameObject serena,
            CinemachineBrain brain, CinemachineCamera vcam, Canvas canvas)
        {
            // El punto de medida: el equivalente al «sitio de arranque» de
            // Godot. Detrás de Serena, a la altura de la cámara de juego,
            // mirando por el paseo hacia el centro: media pantalla de terreno,
            // un tercio de cielo y el arbolado.
            var view = new GameObject("Punto de medida").transform;
            Vector3 eye = serena.transform.position + new Vector3(3.5f, 2.6f, -5f);
            view.position = eye;
            view.rotation = Quaternion.LookRotation(new Vector3(-0.55f, -0.12f, 1f));

            var go = new GameObject("Banco de medida");
            var bench = go.AddComponent<PerfBenchmark>();
            var so = new SerializedObject(bench);
            so.FindProperty("_view").objectReferenceValue = view;
            so.FindProperty("_camera").objectReferenceValue = brain.GetComponent<Camera>();
            var freeze = so.FindProperty("_freeze");
            var toFreeze = new Behaviour[]
            {
                brain, serena.GetComponent<PlayerMotor>(), canvas.GetComponentInChildren<TouchControls>(),
            };
            freeze.arraySize = toFreeze.Length;
            for (int i = 0; i < toFreeze.Length; i++) freeze.GetArrayElementAtIndex(i).objectReferenceValue = toFreeze[i];
            so.FindProperty("_terrain").objectReferenceValue = terrain;
            so.FindProperty("_decor").objectReferenceValue = decor;
            so.FindProperty("_sun").objectReferenceValue = sun;
            so.FindProperty("_player").objectReferenceValue = serena;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------

        static Sprite CircleSprite()
        {
            const string path = "Assets/_Project/Art/UI/circle.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n / 2f - d)));
            }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static RectTransform Img(string name, Transform parent, Sprite sprite, Color color, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            return rt;
        }

        static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
