using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SailorMoon.EditorTools.Import
{
    /// <summary>
    /// Pone en pose T la referencia de cada avatar Humanoid, que es lo que hace
    /// a mano el botón «Enforce T-Pose» del configurador de avatares de Unity.
    ///
    /// POR QUÉ: Unity traduce las animaciones de un cuerpo a otro pasando por
    /// un espacio común, y ese espacio da por hecho que cada avatar está en
    /// pose T. Mixamo no lo garantiza: los FBX «con piel» (el modelo de Serena,
    /// `idle`) vienen en la pose del modelo, en A, con los brazos unos 50° hacia
    /// abajo; los «sin piel», en T. Con un avatar en A, cada animación se
    /// traducía torcida: rodillas juntas, pies girados, la cadera pellizcando
    /// la falda (docs/TRAMPAS-UNITY.md, U10). En Godot no pasaba porque allí
    /// las pistas se aplicaban hueso a hueso, sin traducir.
    ///
    /// Solo toca la POSE DE REFERENCIA del avatar, no la malla ni la animación.
    /// Se aplica sola a cada humanoide nuevo (una vez: queda marcado en el
    /// userData del importador) y a mano desde el menú.
    /// </summary>
    class HumanoidTPose : AssetPostprocessor
    {
        const string Marker = "tpose-v2";
        static readonly string[] Folders =
        {
            "Assets/_Project/Art/Characters",
            "Assets/_Project/Art/Animations/Humanoid",
        };

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            var pending = imported.Where(p => p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) &&
                                              Folders.Any(p.StartsWith) && NeedsPose(p)).ToList();
            if (pending.Count == 0) return;
            // Fuera del import en curso: aquí dentro no se puede reimportar.
            // Uno a uno y sin StartAssetEditing: cada paso necesita el
            // resultado del reimport anterior.
            EditorApplication.delayCall += () =>
            {
                foreach (var p in pending) Apply(p, force: false);
            };
        }

        static bool NeedsPose(string path) =>
            AssetImporter.GetAtPath(path) is ModelImporter mi &&
            mi.animationType == ModelImporterAnimationType.Human &&
            mi.userData != Marker;

        [MenuItem("Sailor Moon/Importación/Forzar pose T en todos los humanoides")]
        static void ApplyAll()
        {
            foreach (var p in AssetDatabase.FindAssets("t:Model", Folders).Select(AssetDatabase.GUIDToAssetPath).ToList())
                Apply(p, force: true);
        }

        public static bool Apply(string path, bool force)
        {
            if (AssetImporter.GetAtPath(path) is not ModelImporter importer) return false;
            if (importer.animationType != ModelImporterAnimationType.Human) return false;
            if (!force && importer.userData == Marker) return false;

            // 1. Descripción del avatar regenerada desde el archivo. Así no se
            //    arrastra nada de configuraciones anteriores (ni posiciones de
            //    otro esqueleto, ni una pose T aplicada dos veces).
            var fresh = importer.humanDescription;
            fresh.human = new HumanBone[0];
            fresh.skeleton = new SkeletonBone[0];
            importer.humanDescription = fresh;
            importer.userData = "tpose-reset";
            importer.SaveAndReimport();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) return false;
            var inst = Object.Instantiate(model);
            inst.name = model.name;
            inst.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var animator = inst.GetComponent<Animator>();
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                {
                    Debug.LogWarning($"[PoseT] {path}: sin avatar humano todavía; se reintentará al reimportar.");
                    return false;
                }

                float before = PoseError(animator);
                SetTPose(animator);

                // 2. Solo las ROTACIONES de los huesos de brazos y piernas. Las
                //    posiciones y el resto de nodos se quedan como los calculó
                //    Unity: el FBX de Serena sale de Blender con el esqueleto
                //    desplazado respecto a la malla, y tocar esas entradas le
                //    dejaba una escala humana de 0,06 (hundida hasta la falda).
                var hd = importer.humanDescription;
                var posed = new Dictionary<string, Quaternion>();
                foreach (var bone in Limbs)
                {
                    var t = animator.GetBoneTransform(bone);
                    if (t != null) posed[t.name] = t.localRotation;
                }
                var skeleton = hd.skeleton;
                for (int i = 0; i < skeleton.Length; i++)
                    if (posed.TryGetValue(skeleton[i].name, out var rot))
                        skeleton[i].rotation = rot;
                hd.skeleton = skeleton;
                importer.humanDescription = hd;
                importer.userData = Marker;
                importer.SaveAndReimport();
                Debug.Log($"[PoseT] {System.IO.Path.GetFileName(path)}: brazos y piernas a pose T (desvío previo {before:0}°).");
                return true;
            }
            finally
            {
                Object.DestroyImmediate(inst);
            }
        }

        static readonly HumanBodyBones[] Limbs =
        {
            HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
        };

        /// Brazos horizontales hacia cada lado, piernas rectas hacia abajo. El
        /// padre antes que el hijo, porque girar el padre mueve al hijo.
        static void SetTPose(Animator a)
        {
            Vector3 right = a.transform.right, down = -a.transform.up;
            Align(a, HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, down);
            Align(a, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, down);
            Align(a, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, down);
            Align(a, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, down);

            Align(a, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, -right);
            Align(a, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, -right);
            Align(a, HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, -right);
            Align(a, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, right);
            Align(a, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, right);
            Align(a, HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal, right);
        }

        static void Align(Animator a, HumanBodyBones bone, HumanBodyBones child, Vector3 target)
        {
            var b = a.GetBoneTransform(bone);
            var c = a.GetBoneTransform(child);
            if (b == null || c == null) return;
            Vector3 dir = c.position - b.position;
            if (dir.sqrMagnitude < 1e-8f) return;
            b.rotation = Quaternion.FromToRotation(dir, target) * b.rotation;
        }

        /// Cuánto se aparta el brazo izquierdo de la horizontal, en grados.
        static float PoseError(Animator a)
        {
            var u = a.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var l = a.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (u == null || l == null) return 0;
            return Vector3.Angle(l.position - u.position, -a.transform.right);
        }
    }
}
