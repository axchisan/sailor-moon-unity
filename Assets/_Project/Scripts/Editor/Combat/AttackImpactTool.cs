using System.IO;
using System.Linq;
using SailorMoon.Combat;
using UnityEditor;
using UnityEngine;

namespace SailorMoon.EditorTools.Combat
{
    /// <summary>
    /// Mide en cada clip de ataque el instante del impacto: el momento en que
    /// una mano o un pie llega más lejos por delante del cuerpo. Es cuando el
    /// golpe tiene que conectar.
    ///
    /// POR QUÉ: en Godot el golpe conectaba por tiempo fijo (windup 0,07 s) y
    /// la animación iba por su lado; quedó pendiente sincronizarlos. Aquí el
    /// tiempo sale del propio clip, y si se cambia el clip se vuelve a medir.
    ///
    ///   Sailor Moon ▸ Combate ▸ Crear ataques de Serena   (una vez: crea los assets)
    ///   Sailor Moon ▸ Combate ▸ Medir impactos            (rellena impactTime y clipLength)
    /// </summary>
    static class AttackImpactTool
    {
        const string Folder = "Assets/_Project/Settings/Combat";
        const string Clips = "Assets/_Project/Art/Animations/Humanoid";
        const string MeasureModel = "Assets/_Project/Art/Characters/SailorMars/SailorMars.fbx";

        [MenuItem("Sailor Moon/Combate/Crear ataques de Serena")]
        static void CreateAttacks()
        {
            Directory.CreateDirectory(Folder);
            // Valores de Godot (assets/data/attacks/*.tres), ya probados con la jugadora.
            Make("Serena_Combo1", "Golpe 1", "Attack1", "attack_1", speed: 2.2f, recovery: 0.13f,
                 damage: 1, knock: 3f, up: 0f, lunge: 2.2f, stop: 0.045f, shake: 0.15f, heavy: false, radius: 0.9f, reach: 1.0f);
            Make("Serena_Combo2", "Golpe 2", "Attack2", "attack_2", speed: 2.4f, recovery: 0.14f,
                 damage: 1, knock: 3.6f, up: 0f, lunge: 2.6f, stop: 0.055f, shake: 0.22f, heavy: false, radius: 0.9f, reach: 1.0f);
            Make("Serena_Combo3", "Patada giratoria", "Attack3", "attack_3", speed: 2.0f, recovery: 0.26f,
                 damage: 2, knock: 9f, up: 3.5f, lunge: 3.2f, stop: 0.10f, shake: 0.5f, heavy: true, radius: 1.1f, reach: 1.1f);
            var sp = Make("Serena_Special", "Por el poder del prisma lunar", "Special", "special", speed: 1.4f, recovery: 0.55f,
                 damage: 4, knock: 12f, up: 4.5f, lunge: 0f, stop: 0.18f, shake: 0.8f, heavy: true, radius: 5f, reach: 0f);
            sp.breaksShield = true;
            sp.specialCharge = 0;
            EditorUtility.SetDirty(sp);
            AssetDatabase.SaveAssets();
            MeasureAll();
        }

        static AttackData Make(string file, string name, string state, string clip, float speed, float recovery,
            int damage, float knock, float up, float lunge, float stop, float shake, bool heavy, float radius, float reach)
        {
            string path = $"{Folder}/{file}.asset";
            var a = AssetDatabase.LoadAssetAtPath<AttackData>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<AttackData>();
                AssetDatabase.CreateAsset(a, path);
            }
            a.displayName = name; a.animatorState = state; a.clipName = clip; a.animSpeed = speed;
            a.recovery = recovery; a.damage = damage; a.knockback = knock; a.knockbackUp = up; a.lunge = lunge;
            a.hitStop = stop; a.shake = shake; a.heavy = heavy; a.radius = radius; a.reach = reach;
            // Anticipación visible: rápida en el combo (machacar), más en la patada
            // y en el especial, que se tiene que ver venir.
            a.targetWindup = heavy ? (reach == 0f ? 0.5f : 0.22f) : 0.14f;
            EditorUtility.SetDirty(a);
            return a;
        }

        [MenuItem("Sailor Moon/Combate/Medir impactos")]
        static void MeasureAll()
        {
            var report = new System.Text.StringBuilder("[Combate] Impactos medidos:\n");
            // Por carpeta y no por «t:AttackData»: la búsqueda por tipo no siempre
            // encuentra ScriptableObjects recién creados.
            foreach (var guid in AssetDatabase.FindAssets("", new[] { Folder }))
            {
                var a = AssetDatabase.LoadAssetAtPath<AttackData>(AssetDatabase.GUIDToAssetPath(guid));
                if (a == null) continue;
                var clip = LoadClip(a.clipName);
                if (clip == null) { report.AppendLine($"  ⚠ {a.name}: no hay clip {a.clipName}"); continue; }
                var (t, limb, reach) = MeasureImpact(clip);
                a.impactTime = t;
                a.clipLength = clip.length;
                a.startTime = Mathf.Max(0f, t - a.targetWindup * a.animSpeed);
                EditorUtility.SetDirty(a);
                report.AppendLine($"  {a.name,-16} {a.clipName,-10} impacto {t:0.00} s de {clip.length:0.00} s, desde {a.startTime:0.00} " +
                                  $"({limb}, {reach:0.00} m) → anticipación real {a.Windup:0.00} s, total {a.Duration:0.00} s");
            }
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        static AnimationClip LoadClip(string name) =>
            AssetDatabase.LoadAllAssetsAtPath($"{Clips}/{name}.fbx").OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview"));

        /// Muestrea el clip sobre un personaje y busca la extremidad que más se
        /// adelanta respecto a la cadera, y cuándo.
        static (float time, string limb, float reach) MeasureImpact(AnimationClip clip)
        {
            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(MeasureModel));
            var an = go.GetComponent<Animator>();
            var limbs = new[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            float bestReach = -1, bestTime = clip.length * 0.3f;
            string bestLimb = "-";
            AnimationMode.StartAnimationMode();
            try
            {
                const int N = 90;
                for (int i = 0; i <= N; i++)
                {
                    float t = clip.length * i / N;
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(go, clip, t);
                    AnimationMode.EndSampling();
                    var hips = an.GetBoneTransform(HumanBodyBones.Hips);
                    foreach (var b in limbs)
                    {
                        var tr = an.GetBoneTransform(b);
                        // Hacia delante del personaje (+Z), en horizontal.
                        float forward = go.transform.InverseTransformPoint(tr.position).z
                                      - go.transform.InverseTransformPoint(hips.position).z;
                        if (forward > bestReach) { bestReach = forward; bestTime = t; bestLimb = b.ToString(); }
                    }
                }
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                Object.DestroyImmediate(go);
            }
            return (bestTime, bestLimb, bestReach);
        }
    }
}
