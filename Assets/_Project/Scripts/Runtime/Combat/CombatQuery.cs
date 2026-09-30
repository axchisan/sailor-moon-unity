using System.Collections.Generic;
using UnityEngine;

namespace SailorMoon.Combat
{
    /// <summary>
    /// Las dos preguntas del combate: ¿a quién apunto? y ¿a quién le he dado?
    /// Sin colisionadores de golpe permanentes: se pregunta a la física solo
    /// mientras la ventana del golpe está abierta.
    /// </summary>
    public static class CombatQuery
    {
        static readonly Collider[] Buffer = new Collider[32];

        /// <summary>
        /// El objetivo al que girarse al atacar. Puntúa por distancia Y ángulo
        /// respecto a hacia donde se quiere ir: prefiere lo que hay delante,
        /// pero nunca deja golpear al aire por estar 5° desviada.
        ///
        /// Sin esto, un beat 'em up en 3D con joystick virtual es injugable a
        /// los 8 años (GDD §5.3). La fórmula es la de Godot, ya probada.
        /// </summary>
        public static IDamageable FindTarget(Vector3 origin, Vector3 aim, float range, int mask, Team attacker)
        {
            int n = Physics.OverlapSphereNonAlloc(origin, range, Buffer, mask, QueryTriggerInteraction.Ignore);
            IDamageable best = null;
            float bestScore = float.MaxValue;
            aim.y = 0;
            if (aim.sqrMagnitude < 1e-4f) aim = Vector3.forward;
            aim.Normalize();
            for (int i = 0; i < n; i++)
            {
                var d = Buffer[i].GetComponentInParent<IDamageable>();
                if (d == null || d.Team == attacker || !d.CanBeHit) continue;
                Vector3 to = d.transform.position - origin;
                to.y = 0;
                float dist = to.magnitude;
                if (dist > range) continue;
                float angle = dist > 0.05f ? Vector3.Angle(aim, to) * Mathf.Deg2Rad : 0f;
                // El ángulo penaliza pero no descarta: mejor pegarle a algo que fallar.
                float score = dist + angle * 1.6f;
                if (score < bestScore) { bestScore = score; best = d; }
            }
            return best;
        }

        /// <summary>
        /// Todo lo que cae dentro de la zona de un golpe y se puede golpear,
        /// sin atravesar paredes: un rayo contra el escenario desde el pecho del
        /// que golpea hasta el objetivo (trampa 108 de Godot: toda barrera que
        /// para los pies tiene que parar los golpes).
        /// </summary>
        public static int Overlap(Vector3 center, float radius, int mask, Team attacker, Vector3 chest,
                                  List<IDamageable> results)
        {
            results.Clear();
            int n = Physics.OverlapSphereNonAlloc(center, radius, Buffer, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var d = Buffer[i].GetComponentInParent<IDamageable>();
                if (d == null || d.Team == attacker || results.Contains(d)) continue;
                Vector3 target = Buffer[i].bounds.center;
                if (Physics.Linecast(chest, target, Layers.World, QueryTriggerInteraction.Ignore)) continue;
                results.Add(d);
            }
            return results.Count;
        }
    }
}
