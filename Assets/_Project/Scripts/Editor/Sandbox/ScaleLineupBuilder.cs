using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SailorMoon.EditorTools.Sandbox
{
    /// <summary>
    /// La sala de escala: todos los modelos en fila junto a Serena, con una
    /// regla de metros detrás, y fotos con cámara ortográfica (sin
    /// perspectiva, así la proporción que se ve es la real).
    ///
    /// Es la comprobación de docs/ESCALA.md §5: **ningún modelo entra en un
    /// escenario sin haber pasado por aquí.** Se rehace entera cada vez, así
    /// que recoge lo que haya en Art/ en ese momento.
    ///
    ///   Sailor Moon ▸ Sandbox ▸ Sala de escala           (montar)
    ///   Sailor Moon ▸ Sandbox ▸ Fotos de la sala de escala  (Captures/escala/)
    /// </summary>
    static class ScaleLineupBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Sandbox/ScaleLineup.unity";
        const string Art = "Assets/_Project/Art";
        const float Gap = 0.8f;
        const float RowDepth = 16f;

        [MenuItem("Sailor Moon/Sandbox/Sala de escala")]
        public static void Build()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.isDirty && !string.IsNullOrEmpty(active.path)) EditorSceneManager.SaveScene(active);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Sol").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40, -30, 0);
            sun.shadows = LightShadows.None;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Suelo";
            floor.transform.localScale = new Vector3(40, 1, 12);
            floor.transform.position = new Vector3(150, 0, 30);
            floor.GetComponent<Renderer>().sharedMaterial = Flat(new Color(0.82f, 0.84f, 0.8f), "Suelo");

            // Filas: personajes; enemigos; decorado pequeño; decorado grande.
            var rows = new List<(string title, string[] paths)>
            {
                ("Personajes", Models($"{Art}/Characters").Where(IsCharacter).ToArray()),
                ("Enemigos y jefes", Models($"{Art}/Characters").Where(p => !IsCharacter(p)).ToArray()),
                ("Decorado menor de 2,5 m", Models($"{Art}/Props").Where(p => Height(p) < 2.5f).ToArray()),
                ("Decorado grande", Models($"{Art}/Props").Where(p => Height(p) >= 2.5f).ToArray()),
            };

            var serena = AssetDatabase.LoadAssetAtPath<GameObject>($"{Art}/Characters/Serena/Serena.fbx");
            float z = 0;
            foreach (var (title, paths) in rows)
            {
                var row = new GameObject(title).transform;
                row.position = new Vector3(0, 0, z);
                float x = 0;
                // Serena al principio de cada fila, como referencia.
                x = Place(serena, row, x, "Serena (1,55 m)");
                foreach (var p in paths)
                    x = Place(AssetDatabase.LoadAssetAtPath<GameObject>(p), row, x, Path.GetFileNameWithoutExtension(p));
                Ruler(row, x);
                z += RowDepth;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Sala de escala] Montada. Fotos: Sailor Moon ▸ Sandbox ▸ Fotos de la sala de escala.");
        }

        [MenuItem("Sailor Moon/Sandbox/Fotos de la sala de escala")]
        public static void Photos()
        {
            Directory.CreateDirectory("Captures/escala");
            var camGo = new GameObject("CamaraFoto");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.94f, 0.97f);
            try
            {
                foreach (Transform row in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                             .Where(t => t.parent == null && t.Find("Regla") != null))
                {
                    var b = Bounds(row);
                    float h = Mathf.Max(b.size.y, 2f) + 1f;
                    float w = b.size.x + 2f;
                    const int px = 2400;
                    int py = Mathf.Clamp(Mathf.RoundToInt(px * h / w), 300, 1600);
                    cam.orthographicSize = Mathf.Max(h / 2f, w * py / px / 2f);
                    cam.transform.position = new Vector3(b.center.x, h / 2f - 0.3f, row.position.z - 20f);
                    cam.transform.rotation = Quaternion.identity;
                    // Solo esta fila: una ortográfica ve todo lo que hay detrás.
                    cam.nearClipPlane = 12f;
                    cam.farClipPlane = 12f + RowDepth - 2f;
                    var rt = new RenderTexture(px, py, 24);
                    cam.targetTexture = rt;
                    cam.aspect = (float)px / py;
                    cam.Render();
                    RenderTexture.active = rt;
                    var tex = new Texture2D(px, py, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, px, py), 0, 0);
                    tex.Apply();
                    File.WriteAllBytes($"Captures/escala/sala_{row.name.Replace(' ', '_')}.png", tex.EncodeToPNG());
                    RenderTexture.active = null;
                    cam.targetTexture = null;
                    Object.DestroyImmediate(tex);
                    rt.Release();
                }
            }
            finally { Object.DestroyImmediate(camGo); }
            Debug.Log("[Sala de escala] Fotos en Captures/escala/");
        }

        static float Place(GameObject model, Transform row, float x, string label)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model, row);
            go.transform.localPosition = Vector3.zero;
            // Se SUMA el giro: los FBX salidos de Blender traen su propia rotación
            // en la raíz, y sustituirla los tumba (la fuente salía de canto).
            go.transform.localRotation = Quaternion.Euler(0, 180, 0) * go.transform.localRotation;
            var b = Bounds(go.transform);
            // Apoyado en el suelo y en su hueco de la fila.
            go.transform.position += new Vector3(x - b.min.x + Gap, -b.min.y, 0);
            if (go.TryGetComponent<Animator>(out var an)) an.enabled = false;
            b = Bounds(go.transform);
            Label(row, $"{label}\n{b.size.y:0.00} m", new Vector3(b.center.x, -0.35f, -1f));
            return b.max.x;
        }

        /// Regla: postes cada metro hasta 10 m, alternando colores, detrás de la fila.
        static void Ruler(Transform row, float length)
        {
            var ruler = new GameObject("Regla");
            ruler.transform.SetParent(row, false);
            var a = Flat(new Color(0.2f, 0.2f, 0.25f), "ReglaA");
            var bm = Flat(new Color(0.95f, 0.5f, 0.6f), "ReglaB");
            for (int m = 0; m < 10; m++)
            {
                var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(line.GetComponent<Collider>());
                line.transform.SetParent(ruler.transform, false);
                line.transform.localScale = new Vector3(length + 2 * Gap, 0.02f, 0.02f);
                line.transform.localPosition = new Vector3((length + 2 * Gap) / 2f, m + 1, 3f);
                line.GetComponent<Renderer>().sharedMaterial = m % 2 == 0 ? a : bm;
                Label(ruler.transform, $"{m + 1} m", new Vector3(-0.6f, m + 1, 3f));
            }
        }

        static void Label(Transform parent, string text, Vector3 localPos)
        {
            var go = new GameObject("Etiqueta");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.06f;
            tm.fontSize = 40;
            tm.anchor = TextAnchor.UpperCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.1f, 0.1f, 0.15f);
        }

        static Material Flat(Color c, string name)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
            m.SetColor("_BaseColor", c);
            return m;
        }

        static IEnumerable<string> Models(string folder) =>
            AssetDatabase.FindAssets("t:Model", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p);

        static bool IsCharacter(string p) =>
            AssetImporter.GetAtPath(p) is ModelImporter mi && mi.animationType == ModelImporterAnimationType.Human
            || p.Contains("/Luna/") || p.Contains("/Artemis/");

        static float Height(string p)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p));
            try { return Bounds(go.transform).size.y; }
            finally { Object.DestroyImmediate(go); }
        }

        static Bounds Bounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
