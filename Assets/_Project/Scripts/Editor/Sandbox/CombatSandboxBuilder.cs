using System.Linq;
using SailorMoon.Combat;
using SailorMoon.EditorTools.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SailorMoon.EditorTools.Sandbox
{
    /// <summary>
    /// La escena de pruebas del combate: un claro llano, Serena, cinco muñecos
    /// de entrenamiento y una roca para comprobar que no se pega a través de
    /// las paredes. Es donde se afina el combate (Fase 1) antes de tener
    /// enemigos de verdad (Fase 2).
    ///
    ///   Sailor Moon ▸ Sandbox ▸ Crear escena de combate
    /// </summary>
    static class CombatSandboxBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Sandbox/CombatSandbox.unity";
        const string DummyPrefab = "Assets/_Project/Prefabs/Enemies/TrainingDummy.prefab";

        [MenuItem("Sailor Moon/Sandbox/Crear escena de combate")]
        static void Build()
        {
            var current = EditorSceneManager.GetActiveScene();
            if (current.isDirty && !string.IsNullOrEmpty(current.path)) EditorSceneManager.SaveScene(current);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Luz en tiempo real: aquí no se hornea nada.
            var sun = new GameObject("Sol").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.90f);
            RenderSettings.ambientEquatorColor = new Color(0.64f, 0.66f, 0.70f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.44f, 0.38f);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Scenes/Sandbox/PerfTest/Sky.mat");

            // Suelo: un plano de 60 m con la hierba del parque.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.localScale = new Vector3(6, 1, 6);
            var grass = new Material(Shader.Find("SailorMoon/Toon"));
            grass.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Terrain/Surfaces/Grass_Albedo.png"));
            grass.SetTextureScale("_BaseMap", new Vector2(4.4f, 4.4f)); // 60 m / 13,6 m por repetición
            grass.SetFloat("_OutlineWidth", 0);
            grass.SetShaderPassEnabled("SRPDefaultUnlit", false);
            AssetDatabase.CreateAsset(grass, "Assets/_Project/Scenes/Sandbox/CombatGround.mat");
            ground.GetComponent<Renderer>().sharedMaterial = grass;

            // Una roca: detrás de ella los golpes no deben conectar.
            var rock = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Props/roca_grande.prefab");
            if (rock != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(rock);
                r.transform.position = new Vector3(6, 0, 6);
            }

            var serena = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Serena.prefab"));
            serena.transform.position = Vector3.zero;

            var dummy = Dummy();
            var positions = new[]
            {
                new Vector3(0, 0, 3.5f), new Vector3(2.5f, 0, 5f), new Vector3(-2.5f, 0, 5f),
                new Vector3(-5f, 0, 2f), new Vector3(8f, 0, 8.5f), // la última, detrás de la roca
            };
            var group = new GameObject("Muñecos").transform;
            foreach (var p in positions)
            {
                var d = (GameObject)PrefabUtility.InstantiatePrefab(dummy, group);
                d.transform.position = p;
                d.transform.rotation = Quaternion.LookRotation(-p.normalized);
            }

            PerfTestSceneBuilder.BuildCamera(serena);
            PerfTestSceneBuilder.BuildUI(serena);
            PerfTestSceneBuilder.BuildCombatFeel();

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            // Primera en la build: es la que se juega; el parque de medida va detrás
            // (F2 o cuatro dedos para cambiar de escena, PerfOverlay).
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[Combate] Escena montada: J/clic ataca, K especial, F1 panel.");
        }

        /// Muñeco: el Peluchín de siempre, con física y vida, en la capa Enemy.
        static GameObject Dummy()
        {
            AssetDatabase.DeleteAsset(DummyPrefab); // se rehace siempre: es barato y así recoge los cambios
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DummyPrefab));

            var root = new GameObject("TrainingDummy") { layer = Layers.Enemy };
            var model = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/Peluchin/Peluchin.fbx"), root.transform);
            model.name = "Model";
            var mat = PeluchinMaterial();
            foreach (var r in model.GetComponentsInChildren<Renderer>())
                r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();

            var col = root.AddComponent<CapsuleCollider>();
            col.radius = 0.45f;
            col.height = 0.9f;
            col.center = new Vector3(0, 0.45f, 0);
            var body = root.AddComponent<Rigidbody>();
            body.mass = 20f;
            // Freno: sin él, un empujón de 9 m/s lo mandaba a 14 m resbalando.
            body.linearDamping = 3f;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            var health = root.AddComponent<Health>();
            var hso = new SerializedObject(health);
            hso.FindProperty("_max").intValue = 6;
            hso.FindProperty("_invulnerableAfterHit").floatValue = 0f; // cada golpe del combo entra
            hso.ApplyModifiedPropertiesWithoutUndo();
            root.AddComponent<TrainingDummy>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, DummyPrefab);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static Material PeluchinMaterial()
        {
            const string path = "Assets/_Project/Art/Characters/Peluchin/Peluchin_Toon.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("SailorMoon/Toon"));
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_Project/Art/Characters/Peluchin/Peluchin_Image_0.png"));
            mat.SetFloat("_OutlineWidth", 0.006f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
