using System;
using UnityEngine;

namespace SailorMoon.Combat
{
    /// <summary>
    /// Vida en corazones, con invulnerabilidad tras cada golpe.
    ///
    /// La usan igual Serena, las Pesadillas y los jefes. Lo que pasa al recibir
    /// un golpe o al quedarse sin vida lo decide quien escucha los eventos
    /// (PlayerCombat marea a Serena; una Pesadilla se deshace en Chispas).
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Tooltip("Corazones al empezar.")]
        [SerializeField] int _max = 5;
        [Tooltip("Segundos de invulnerabilidad tras recibir un golpe (GDD: 1,5 s para Serena).")]
        [SerializeField] float _invulnerableAfterHit = 1.5f;

        float _invulnerableUntil;
        bool _forcedInvulnerable;

        public int Max => _max;
        public int Current { get; private set; }
        public bool IsDepleted => Current <= 0;
        public bool IsInvulnerable => _forcedInvulnerable || Time.time < _invulnerableUntil;

        /// (golpe, vida que queda)
        public event Action<HitInfo, int> Damaged;
        public event Action<HitInfo> Depleted;
        public event Action Refilled;

        void Awake() => Current = _max;

        /// Aplica un golpe. Devuelve true si entró.
        public bool Apply(in HitInfo hit)
        {
            if (IsDepleted || IsInvulnerable) return false;
            Current = Mathf.Max(0, Current - Mathf.Max(1, hit.attack != null ? hit.attack.damage : 1));
            _invulnerableUntil = Time.time + _invulnerableAfterHit;
            Damaged?.Invoke(hit, Current);
            if (IsDepleted) Depleted?.Invoke(hit);
            return true;
        }

        public void Refill()
        {
            Current = _max;
            _invulnerableUntil = 0;
            Refilled?.Invoke();
        }

        public void Heal(int hearts) => Current = Mathf.Min(_max, Current + hearts);

        /// Invulnerable mientras dure algo (el especial, una cinemática).
        public void SetInvulnerable(bool on) => _forcedInvulnerable = on;

        public void SetMax(int max, bool refill)
        {
            _max = Mathf.Max(1, max);
            if (refill) Current = _max;
            else Current = Mathf.Min(Current, _max);
        }
    }
}
