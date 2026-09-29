using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SailorMoon.EditorTools
{
    /// <summary>
    /// Toda la configuración del proyecto, escrita como código.
    ///
    /// POR QUÉ EXISTE: en Godot, varios ajustes vivían solo en el editor y se
    /// perdían o se pisaban (las rutas de Android que el editor borraba al
    /// cerrarse, el preset de exportación sin su sección de opciones). Aquí
    /// cada ajuste está en un sitio, con su motivo al lado, y se reaplica igual
    /// en cualquier máquina:
    ///
    ///   Menú  Sailor Moon ▸ Configurar proyecto
    ///   CLI   Unity -batchmode -projectPath . -executeMethod
    ///         SailorMoon.EditorTools.ProjectSetup.ApplyFromCommandLine -quit
    ///
    /// Es idempotente: ejecutarlo dos veces deja lo mismo que una.
    /// </summary>
    public static class ProjectSetup
    {
        public const string AndroidId = "com.familia.sailormoon.unity";
        const string RenderingFolder = "Assets/_Project/Settings/Rendering";
        const string PipelinePath = RenderingFolder + "/URP_Mobile.asset";
        const string RendererPath = RenderingFolder + "/URP_Mobile_Renderer.asset";

        [MenuItem("Sailor Moon/Configurar proyecto", priority = 0)]
        public static void ApplyAll()
        {
            ApplyPlayerSettings();
            ApplyAndroidSettings();
            var pipeline = EnsurePipelineAsset();
            ApplyQualitySettings(pipeline);
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] Configuración aplicada.");
        }

        public static void ApplyFromCommandLine()
        {
            ApplyAll();
            EditorApplication.Exit(0);
        }

        static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Familia";
            PlayerSettings.productName = "Sailor Moon";

            // Lineal: los colores del cel shading salen como se pintaron. En
            // Godot, el tonemap Filmic desaturaba la falda y el lazo (trampa 113).
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // Solo el Input System nuevo: acciones con nombre en vez de teclas
            // sueltas, que es lo que ya se aprendió (trampa 27).
            var projectSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (projectSettings.Length > 0)
            {
                var so = new SerializedObject(projectSettings[0]);
                var input = so.FindProperty("activeInputHandler");
                if (input != null)
                {
                    input.intValue = 1; // 0 = antiguo, 1 = nuevo, 2 = ambos
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Tiempos de CPU y GPU por fotograma (FrameTimingManager): el
            // medidor los necesita, y los fps solos topan en 60 y no dicen nada.
            PlayerSettings.enableFrameTimingStats = true;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        }

        static void ApplyAndroidSettings()
        {
            var android = NamedBuildTarget.Android;

            // Id distinto del de Godot (com.familia.sailormoon): así las dos
            // versiones conviven en el teléfono y no se pisan las partidas.
            PlayerSettings.SetApplicationIdentifier(android, AndroidId);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            // Vulkan primero; GLES3 de respaldo por si algún aparato lo necesita.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });

            // Reparto de fotogramas con Swappy: menos tirones con vsync.
            PlayerSettings.Android.optimizedFramePacing = true;

            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        }

        /// <summary>
        /// El asset de URP, afinado para la Adreno 619 a 1610×720.
        ///
        /// Cada ajuste responde a algo medido en el teléfono con Godot
        /// (docs/aprendido/RENDIMIENTO.md): en esa GPU manda el coste por píxel
        /// y el ancho de banda, no los triángulos.
        /// </summary>
        static UniversalRenderPipelineAsset EnsurePipelineAsset()
        {
            Directory.CreateDirectory(RenderingFolder);

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }
            // Forward clásico: con una luz principal y casi ninguna más,
            // Forward+ solo añade el coste de sus clústeres.
            renderer.renderingMode = RenderingMode.Forward;
            renderer.depthPrimingMode = DepthPrimingMode.Disabled;
            EditorUtility.SetDirty(renderer);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            var so = new SerializedObject(pipeline);
            // Resolución nativa. Bajarla emborrona el contorno del cel shading:
            // lo rechazaste en Godot y sigue rechazado (trampa 63).
            Set(so, "m_RenderScale", 1.0f);
            // Sin HDR: el cel shading no lo necesita y en una GPU por tiles es
            // el doble de ancho de banda por píxel.
            Set(so, "m_SupportsHDR", false);
            // MSAA 4x en vez de FXAA: en una GPU por tiles se resuelve dentro
            // del tile y sale casi gratis. El FXAA fue lo más caro medido en
            // Godot, porque relee el fotograma entero.
            Set(so, "m_MSAA", 4);
            // Nada que obligue a copiar el fotograma a medias.
            Set(so, "m_RequireDepthTexture", false);
            Set(so, "m_RequireOpaqueTexture", false);
            Set(so, "m_StoreActionsOptimization", 0); // Auto

            // Luz: una principal por píxel y hasta 2 puntuales por vértice.
            Set(so, "m_MainLightRenderingMode", 1);        // PerPixel
            Set(so, "m_AdditionalLightsRenderingMode", 2); // PerVertex
            Set(so, "m_AdditionalLightsPerObjectLimit", 2);
            Set(so, "m_AdditionalLightShadowsSupported", false);

            // Sombra: una cascada y alcance corto. En Godot el pase de sombra
            // llegó a costar el triple que la imagen (trampa 50). Lo lejano va
            // horneado en lightmaps, así que la sombra en tiempo real solo
            // tiene que cubrir lo que se mueve cerca de la cámara.
            Set(so, "m_MainLightShadowsSupported", true);
            Set(so, "m_MainLightShadowmapResolution", 2048);
            Set(so, "m_ShadowDistance", 30f);
            Set(so, "m_ShadowCascadeCount", 1);
            Set(so, "m_SoftShadowsSupported", true);
            Set(so, "m_SoftShadowQuality", 1); // Low: quita el dentado sin coste de geometría (trampa 73)

            Set(so, "m_MixedLightingSupported", true); // Lightmaps + sombra en tiempo real
            Set(so, "m_SupportsTerrainHoles", false);  // Una variante de shader menos
            Set(so, "m_UseSRPBatcher", true);
            Set(so, "m_SupportsDynamicBatching", false);
            Set(so, "m_ColorGradingMode", 0);  // LDR
            Set(so, "m_ColorGradingLutSize", 16);
            Set(so, "m_EnableLODCrossFade", false);
            Set(so, "m_UseAdaptivePerformance", true);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            return pipeline;
        }

        /// <summary>
        /// Un solo nivel de calidad para todas las plataformas.
        ///
        /// En Godot, el perfil de móvil cambiaba el ASPECTO y no solo el coste,
        /// y el móvil acabó viéndose peor sin que nadie lo decidiera (trampa
        /// 114). Con un único nivel, lo que se ve en el editor del Mac es lo
        /// que se ve en el teléfono.
        /// </summary>
        static void ApplyQualitySettings(UniversalRenderPipelineAsset pipeline)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset");
            if (assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var levels = so.FindProperty("m_QualitySettings");
            levels.arraySize = 1;
            var level = levels.GetArrayElementAtIndex(0);
            level.FindPropertyRelative("name").stringValue = "Movil";
            level.FindPropertyRelative("customRenderPipeline").objectReferenceValue = pipeline;
            level.FindPropertyRelative("vSyncCount").intValue = 1;
            level.FindPropertyRelative("lodBias").floatValue = 1f;
            level.FindPropertyRelative("anisotropicTextures").intValue = 1; // Por textura
            level.FindPropertyRelative("globalTextureMipmapLimit").intValue = 0;
            level.FindPropertyRelative("skinWeights").intValue = 4;
            level.FindPropertyRelative("realtimeReflectionProbes").boolValue = false;
            so.FindProperty("m_CurrentQuality").intValue = 0;

            var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
            for (int i = 0; i < perPlatform.arraySize; i++)
                perPlatform.GetArrayElementAtIndex(i).FindPropertyRelative("second").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Set(SerializedObject so, string name, float value)
        {
            var p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] No existe {name} en esta versión de URP."); return; }
            if (p.propertyType == SerializedPropertyType.Float) p.floatValue = value;
            else p.intValue = (int)value;
        }

        static void Set(SerializedObject so, string name, bool value)
        {
            var p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning($"[ProjectSetup] No existe {name} en esta versión de URP."); return; }
            p.boolValue = value;
        }
    }
}
