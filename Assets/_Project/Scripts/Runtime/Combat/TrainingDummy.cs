using System.Collections;
using UnityEngine;

namespace SailorMoon.Combat
{
    /// <summary>
    /// Muñeco de entrenamiento para afinar el combate sin enemigos de verdad:
    /// recibe golpes, sale despedido, destella y, al quedarse sin vida, gira,
    /// se encoge, estalla en chispas (GDD §5.7) y vuelve a aparecer.
    ///
    /// Usa un Rigidbody para el empujón: así se ve con claridad cuánto empuja
    /// cada golpe (el tercero tiene que mandarlo lejos).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        static readonly int FlashId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] float _respawnDelay = 2.5f;
        [Tooltip("Segundos del destello blanco al recibir (GDD: 80 ms).")]
        [SerializeField] float _flashTime = 0.08f;

        Rigidbody _body;
        Health _health;
        Renderer[] _renderers;
        MaterialPropertyBlock _mpb;
        Vector3 _home;
        Quaternion _homeRot;
        Vector3 _scale;
        bool _defeated;

        public Team Team => Team.Enemy;
        public bool CanBeHit => !_defeated;

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _renderers = GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _home = transform.position;
            _homeRot = transform.rotation;
            _scale = transform.localScale;
            _health.Depleted += _ => StartCoroutine(Defeat());
        }

        public bool TakeHit(in HitInfo hit)
        {
            if (_defeated) return false;
            if (!_health.Apply(hit)) return false;
            if (hit.attack != null)
                _body.linearVelocity = hit.direction * hit.attack.knockback + Vector3.up * (hit.attack.knockbackUp + 1.5f);
            StartCoroutine(Flash());
            return true;
        }

        IEnumerator Flash()
        {
            SetFlash(1f);
            yield return new WaitForSecondsRealtime(_flashTime);
            SetFlash(0f);
        }

        void SetFlash(float amount)
        {
            foreach (var r in _renderers)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetFloat(FlashId, amount);
                r.SetPropertyBlock(_mpb);
            }
        }

        IEnumerator Defeat()
        {
            _defeated = true;
            var constraints = _body.constraints;
            _body.constraints = RigidbodyConstraints.None;
            _body.linearVelocity += Vector3.up * 4f;
            _body.angularVelocity = Random.onUnitSphere * 12f;
            // Sale despedido, gira y se encoge: nada desagradable.
            for (float t = 0; t < 0.6f; t += Time.deltaTime)
            {
                transform.localScale = _scale * (1f - t / 0.6f);
                yield return null;
            }
            CombatFeel.Instance?.Defeat(transform.position + Vector3.up * 0.5f);
            foreach (var r in _renderers) r.enabled = false;
            _body.isKinematic = true;
            yield return new WaitForSeconds(_respawnDelay);

            transform.SetPositionAndRotation(_home, _homeRot);
            transform.localScale = _scale;
            _body.isKinematic = false;
            _body.constraints = constraints;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            foreach (var r in _renderers) r.enabled = true;
            _health.Refill();
            _defeated = false;
        }
    }
}
