using UnityEngine;

namespace SailorMoon.Combat
{
    public enum Team { Player, Enemy }

    /// Todo lo que viaja con un golpe.
    public struct HitInfo
    {
        public AttackData attack;
        public GameObject source;
        public Team sourceTeam;
        /// Dirección horizontal del empuje (normalizada).
        public Vector3 direction;
        /// Punto de impacto, para las partículas.
        public Vector3 point;
    }

    /// <summary>
    /// Lo que puede recibir golpes: Serena, las Pesadillas, los jefes, los
    /// muñecos de entrenamiento. Una sola puerta para el daño, así las reglas
    /// (invulnerabilidad, escudos, equipos) viven en un sitio.
    /// </summary>
    public interface IDamageable
    {
        Team Team { get; }
        bool CanBeHit { get; }
        /// Devuelve true si el golpe entró (no bloqueado, no invulnerable).
        bool TakeHit(in HitInfo hit);
        Transform transform { get; }
    }

    /// Capas de física del juego. Las pone ProjectSetup; aquí, para el código.
    public static class Layers
    {
        public const int Player = 6;
        public const int Enemy = 7;
        public const int Pickup = 8;
        public static int Mask(int layer) => 1 << layer;
        /// Lo que tapa un golpe: el escenario (Default).
        public static readonly int World = 1 << 0;
    }
}
