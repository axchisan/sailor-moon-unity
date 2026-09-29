using System.IO;
using UnityEditor;
using UnityEngine;

namespace SailorMoon.EditorTools.Terrain
{
    /// <summary>
    /// Una superficie del terreno (hierba, camino, piedra…) descrita con tres
    /// colores y unos pocos números, que se hornea a una textura y a una capa
    /// de Unity Terrain.
    ///
    /// POR QUÉ ASÍ: en Godot la misma receta se calculaba en el shader, por
    /// píxel y en cada fotograma, sobre la superficie que ocupa media pantalla.
    /// Aquí el resultado se calcula UNA vez, en el editor, y el terreno lo lee
    /// como una textura normal. El aspecto es el mismo (las recetas vienen de
    /// paleta_terreno.gd) y el coste por píxel es una lectura.
    ///
    /// Para cambiar el tono de la hierba: se retoca este asset en el
    /// Inspector y se pulsa «Hornear». El terreno cambia sin tocar nada más.
    /// </summary>
    [CreateAssetMenu(menuName = "Sailor Moon/Superficie de terreno", fileName = "Surface")]
    public class TerrainSurfaceRecipe : ScriptableObject
    {
        [Tooltip("El color que domina.")]
        public Color baseColor = new(0.43f, 0.58f, 0.36f);
        [Tooltip("El tono oscuro de las manchas.")]
        public Color darkColor = new(0.33f, 0.47f, 0.30f);
        [Tooltip("El claro que asoma solo en la banda más alta del patrón.")]
        public Color lightColor = new(0.56f, 0.70f, 0.42f);

        [Tooltip("Metros que ocupa una repetición de la mancha.")]
        public float scale = 3.4f;
        [Tooltip("Escalones de color. 2 parece un cartel, 5 ya es un degradado y se pierde el estilo.")]
        [Range(2, 6)] public int tones = 3;
        [Tooltip("Cuánto se nota el patrón sobre el color base.")]
        [Range(0, 1)] public float strength = 0.62f;
        [Tooltip("Ruido fino que rompe la repetición de las manchas.")]
        [Range(0, 1)] public float grain = 0.2f;
        [Tooltip("Rayado en una dirección, para tierra pisada o madera.")]
        [Range(0, 1)] public float streaks;

        /// La textura de ruido cubre DOMINIO repeticiones de la mancha: es el
        /// `DOMINIO` de tools/generar_textura_ruido.gd en Godot.
        const int Domain = 4;
        const string NoisePath = "Assets/_Project/Art/Textures/Noise/surface_noise.png";

        /// Metros que ocupa la textura horneada entera en el terreno.
        public float TileSize => scale * Domain;

        public string TexturePath => Path.ChangeExtension(AssetDatabase.GetAssetPath(this), null) + "_Albedo.png";
        public string LayerPath => Path.ChangeExtension(AssetDatabase.GetAssetPath(this), null) + ".terrainlayer";

        [ContextMenu("Hornear")]
        public TerrainLayer Bake()
        {
            var noise = LoadNoise();
            int size = noise.width;
            var src = noise.GetPixels();
            var dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
                dst[i] = Shade(src[i]);

            // Con ALFA 0: el Terrain de URP lee el alfa de la textura como
            // suavidad. Sin canal alfa vale 1 y el suelo se vuelve un espejo
            // que, en cuanto hay una sonda de reflexión (al hornear la luz),
            // refleja cielo blanco o suelo negro según el ángulo.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels(dst);
            File.WriteAllBytes(TexturePath, tex.EncodeToPNG());
            DestroyImmediate(tex);
            AssetDatabase.ImportAsset(TexturePath);

            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(LayerPath);
            if (layer == null)
            {
                layer = new TerrainLayer();
                AssetDatabase.CreateAsset(layer, LayerPath);
            }
            layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            layer.tileSize = new Vector2(TileSize, TileSize);
            layer.metallic = 0;
            layer.smoothness = 0;
            layer.specular = Color.black;
            EditorUtility.SetDirty(layer);
            AssetDatabase.SaveAssets();
            return layer;
        }

        /// La misma cuenta que terreno.gdshader, sin la iluminación.
        Color Shade(Color noise)
        {
            float pattern = streaks > 0.001f ? Mathf.Lerp(noise.r, noise.b, streaks) : noise.r;
            float stepped = Mathf.Clamp01(Mathf.Floor(pattern * tones) / Mathf.Max(tones - 1f, 1f));

            Color c = Color.Lerp(darkColor, baseColor, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.6f, stepped)));
            c = Color.Lerp(c, lightColor, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.72f, 1f, stepped)));
            c = Color.Lerp(baseColor, c, strength);
            c += Color.white * ((noise.g - 0.5f) * grain * 0.14f);
            c.a = 0; // suavidad 0: mate
            return c;
        }

        static Texture2D LoadNoise()
        {
            // Se lee el PNG del disco, no el asset importado: así no hace falta
            // marcarlo legible ni cargar con él en la build.
            var tex = new Texture2D(2, 2, TextureFormat.RGB24, false, linear: true);
            tex.LoadImage(File.ReadAllBytes(NoisePath));
            return tex;
        }
    }

    [CustomEditor(typeof(TerrainSurfaceRecipe))]
    class TerrainSurfaceRecipeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            var recipe = (TerrainSurfaceRecipe)target;
            EditorGUILayout.HelpBox($"La textura cubre {recipe.TileSize:0.#} m del terreno.", MessageType.None);
            if (GUILayout.Button("Hornear", GUILayout.Height(28)))
                recipe.Bake();
        }
    }
}
