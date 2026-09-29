using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SailorMoon.EditorTools.Import
{
    /// <summary>
    /// La altura real de cada modelo del decorado, y la escala de importación
    /// que la consigue.
    ///
    /// POR QUÉ EXISTE: los props vienen de scripts de Blender y de hi3d.ai, y
    /// ninguno sale a su tamaño real. Los cerezos medían 3,7 m (uno de verdad,
    /// 6–9 m) y la Estrella de Sueño 0,9 m: Serena, de 1,55 m, se veía casi tan
    /// alta como un árbol y el parque parecía de juguete (docs/ESCALA.md).
    ///
    /// Cómo se usa:
    ///   1. Cada modelo nuevo tiene una fila con su ALTURA REAL en metros y de
    ///      qué referencia sale (docs/ESCALA.md §3).
    ///   2. «Sailor Moon ▸ Importación ▸ Normalizar escalas» mide cada modelo,
    ///      calcula la escala y reimporta.
    ///   3. La escala queda escrita en el importador de cada modelo (su .meta),
    ///      así que viaja con el repositorio y no depende de nada al importar.
    ///
    /// Los personajes NO van aquí: ya miden lo que deben (se comprobó por
    /// estatura de coronilla a planta del pie).
    /// </summary>
    [CreateAssetMenu(menuName = "Sailor Moon/Tabla de escalas del mundo", fileName = "WorldScale")]
    public class WorldScaleTable : ScriptableObject
    {
        public const string AssetPath = "Assets/_Project/Settings/WorldScale.asset";

        [Serializable]
        public class Entry
        {
            [Tooltip("Nombre del modelo (el del .fbx, sin extensión).")]
            public string model;
            [Tooltip("Altura real que debe tener, en metros.")]
            public float targetHeight = 1f;
            [Tooltip("De dónde sale la medida. Obligatorio: una medida sin referencia es una corazonada.")]
            public string reference;
            [Tooltip("Calculada por «Normalizar escalas». No tocar a mano.")]
            public float importScale = 1f;
        }

        public List<Entry> entries = new();

        public Entry Find(string model) => entries.Find(e => e.model == model);

        public static WorldScaleTable Load() => AssetDatabase.LoadAssetAtPath<WorldScaleTable>(AssetPath);

        /// ¿Tiene fila este modelo? Se lee el YAML del disco, no el asset:
        /// funciona también dentro de un import en proceso paralelo.
        public static bool IsListed(string model)
        {
            if (!File.Exists(AssetPath)) return false;
            return File.ReadAllText(AssetPath).Contains($"model: {model}\n");
        }

        [MenuItem("Sailor Moon/Importación/Normalizar escalas")]
        public static void Normalize()
        {
            var table = Load();
            if (table == null) { Debug.LogError($"[Escala] Falta {AssetPath}"); return; }

            var report = new System.Text.StringBuilder("[Escala] Normalización:\n");
            foreach (var e in table.entries)
            {
                // Altura 0: piezas planas (losas) que se miden por su ancho y no
                // se escalan desde aquí.
                if (e.targetHeight <= 0f) { report.AppendLine($"  {e.model,-20} plano: no se escala"); continue; }
                string path = FindModel(e.model);
                if (path == null) { report.AppendLine($"  ⚠ {e.model}: no existe el modelo"); continue; }

                // Altura a la escala actual → altura a escala 1 → escala buscada.
                float current = MeasureHeight(path);
                if (current <= 0.0001f) { report.AppendLine($"  ⚠ {e.model}: sin mallas"); continue; }
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                float atOne = current / Mathf.Max(importer.globalScale, 1e-6f);
                float scale = e.targetHeight / atOne;
                e.importScale = scale;
                report.AppendLine($"  {e.model,-20} {atOne,5:0.00} m → {e.targetHeight,5:0.00} m  (×{scale:0.00})");
                if (Mathf.Abs(importer.globalScale - scale) > 0.001f)
                {
                    importer.globalScale = scale;
                    importer.SaveAndReimport();
                }
            }
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        static string FindModel(string model)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{model} t:Model", new[] { "Assets/_Project/Art" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == model) return p;
            }
            return null;
        }

        static float MeasureHeight(string path)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            try
            {
                var rs = go.GetComponentsInChildren<Renderer>();
                if (rs.Length == 0) return 0;
                var b = rs[0].bounds;
                foreach (var r in rs) b.Encapsulate(r.bounds);
                return b.size.y;
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
