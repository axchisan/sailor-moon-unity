using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SailorMoon.EditorTools.Lighting
{
    /// <summary>
    /// Hornea la luz de la escena abierta. **Usar siempre esto, no el botón
    /// «Generate Lighting» de Unity.**
    ///
    /// POR QUÉ: el lightmapper descarta un terreno con shader propio
    /// («Instance with no materials was removed from light baking input»,
    /// TRAMPAS U15). Así que durante el horneado cada terreno lleva el
    /// material de terreno de URP, que sí hornea, y al terminar vuelve al suyo
    /// (TerrainToon). Las coordenadas del lightmap del terreno son las mismas
    /// con un shader que con otro.
    /// </summary>
    static class LightBaker
    {
        [MenuItem("Sailor Moon/Hornear luz", priority = 20)]
        public static void BakeOpenScene()
        {
            var terrains = Object.FindObjectsByType<UnityEngine.Terrain>(FindObjectsSortMode.None);
            var original = new Dictionary<UnityEngine.Terrain, Material>();
            var urp = GraphicsSettings.currentRenderPipeline.defaultTerrainMaterial;
            foreach (var t in terrains)
            {
                original[t] = t.materialTemplate;
                t.materialTemplate = urp;
            }
            try
            {
                Lightmapping.Bake();
            }
            finally
            {
                foreach (var (t, m) in original) t.materialTemplate = m;
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }
            Debug.Log($"[Luz] Horneada: {LightmapSettings.lightmaps.Length} lightmaps, {terrains.Length} terrenos.");
        }
    }
}
