using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SailorMoon.EditorTools.Import
{
    /// <summary>
    /// Las superficies compartidas del decorado: madera, piedra, hoja, flor…
    ///
    /// Cada textura que cae en <c>Surfaces/Textures/</c> tiene su material toon
    /// en <c>Surfaces/</c> con el mismo nombre, y todo prop cuyo FBX traiga un
    /// material llamado así se enlaza a él al importarse (ModelImportRules).
    /// Retocar «madera» cambia a la vez el banco, la valla, el puente y el
    /// cartel.
    ///
    /// Es la Fase B de Godot («texturas reciclables») hecha con las piezas de
    /// Unity: allí cada prop llevaba su copia de la textura y cambiar el tono
    /// de la madera era tocar siete archivos.
    /// </summary>
    class SurfaceLibrary : AssetPostprocessor
    {
        public const string Folder = "Assets/_Project/Art/Materials/Surfaces";
        const string TextureFolder = Folder + "/Textures/";

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            bool created = false;
            foreach (var path in imported)
            {
                if (!path.StartsWith(TextureFolder) || !path.EndsWith(".png")) continue;
                string matPath = $"{Folder}/{Path.GetFileNameWithoutExtension(path)}.mat";
                if (File.Exists(matPath)) continue;

                var shader = Shader.Find("SailorMoon/Toon");
                if (shader == null) continue;
                var mat = new Material(shader);
                mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                mat.SetFloat("_OutlineWidth", 0f);
                mat.SetShaderPassEnabled("SRPDefaultUnlit", false);
                AssetDatabase.CreateAsset(mat, matPath);
                created = true;
            }

            // Los props que llegaron en la misma tanda ya se importaron sin
            // encontrar su superficie: se reimportan para que se enlacen.
            if (created)
                EditorApplication.delayCall += ReimportProps;
        }

        [MenuItem("Sailor Moon/Importación/Reenlazar props a las superficies")]
        static void ReimportProps()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Project/Art/Props" }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
        }

        /// Nombre del material → material compartido, para el importador.
        public static Dictionary<string, Material> All()
        {
            var result = new Dictionary<string, Material>();
            if (!AssetDatabase.IsValidFolder(Folder)) return result;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { Folder }))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat != null) result[mat.name] = mat;
            }
            return result;
        }
    }
}
