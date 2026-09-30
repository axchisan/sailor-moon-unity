using System;
using System.Collections.Generic;
using SailorMoon.Combat;
using UnityEngine;

namespace SailorMoon.Player
{
    /// <summary>
    /// El combate de la jugadora: combo, especial, recibir golpes y marearse.
    ///
    /// Reglas del GDD (§5), probadas con la jugadora en Godot:
    /// · Machacar siempre funciona: combo de 3 golpes con una ventana de 0,8 s,
    ///   deliberadamente enorme, y la pulsación se guarda si llega durante un
    ///   golpe.
    /// · Al atacar se gira sola hacia el enemigo más cercano (CombatQuery).
    /// · El especial se carga golpeando (12 golpes), es invulnerable y barre
    ///   todo alrededor. No se recarga a sí mismo.
    /// · Sin game over: con 0 corazones se marea y vuelve con la vida llena.
    ///
    /// El golpe conecta cuando la ANIMACIÓN llega al impacto (AttackData.impactTime,
    /// medido sobre el clip), no a un tiempo inventado.
    /// </summary>
    [DefaultExecutionOrder(-50)] // después de la entrada, antes del motor
    public class PlayerCombat : MonoBehaviour, IDamageable
    {
        static readonly int ActionSpeed = Animator.StringToHash("ActionSpeed");

        [Header("Ataques")]
        [SerializeField] AttackData[] _combo = new AttackData[3];
        [SerializeField] AttackData _special;
        [Tooltip("Segundos tras acabar un golpe en los que el siguiente sigue el combo.")]
        [SerializeField] float _comboWindow = 0.8f;
        [Tooltip("Hasta qué distancia se gira sola hacia un enemigo al atacar.")]
        [SerializeField] float _targetRange = 4.5f;
        [Tooltip("Velocidad máxima del salto hacia el enemigo cuando está lejos para el golpe, en m/s.")]
        [SerializeField] float _magnetMaxSpeed = 9f;

        [Header("Especial")]
        [Tooltip("Golpes conectados para llenar la barra.")]
        [SerializeField] int _specialCost = 12;
        [Tooltip("Radio del especial, en metros.")]
        [SerializeField] float _specialRadius = 5f;

        [Header("Recibir")]
        [Tooltip("Segundos de aturdimiento al recibir un golpe.")]
        [SerializeField] float _hurtTime = 0.35f;
        [Tooltip("Segundos mareada antes de volver con la vida llena.")]
        [SerializeField] float _dizzyTime = 1.6f;

        [SerializeField] Animator _animator;

        PlayerMotor _motor;
        InputReader _input;
        Health _health;
        Renderer[] _renderers;

        enum Mode { Free, Attack, Special, Hurt, Dizzy }
        Mode _mode;
        AttackData _current;
        int _comboIndex = -1;
        float _elapsed;
        float _lastAttackEnd = -10f;
        bool _queued;
        bool _windowDone;
        float _modeUntil;
        readonly List<IDamageable> _hits = new();
        readonly List<IDamageable> _alreadyHit = new();
        Transform _target;

        public Team Team => Team.Player;
        public bool CanBeHit => _mode != Mode.Dizzy && !_health.IsInvulnerable;
        public int SpecialCharge { get; private set; }
        public float Special01 => (float)SpecialCharge / _specialCost;
        public bool SpecialReady => SpecialCharge >= _specialCost;
        public Health Health => _health;

        public event Action SpecialStarted;
        public event Action SpecialEnded;
        public event Action<int> ComboStep;

        void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _input = GetComponent<InputReader>();
            _health = GetComponent<Health>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            _renderers = GetComponentsInChildren<Renderer>();
            _health.Damaged += OnDamaged;
            _health.Depleted += OnDepleted;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            switch (_mode)
            {
                case Mode.Free:
                    if (_input.SpecialPressed && SpecialReady && _motor.IsGrounded) StartSpecial();
                    else if (_input.AttackPressed) StartNextAttack();
                    break;
                case Mode.Attack:
                    if (_input.AttackPressed) _queued = true;
                    if (_input.SpecialPressed && SpecialReady) { EndAttack(); StartSpecial(); break; }
                    TickAttack(dt);
                    break;
                case Mode.Special:
                    TickAttack(dt);
                    break;
                case Mode.Hurt:
                case Mode.Dizzy:
                    if (Time.time >= _modeUntil) Recover();
                    break;
            }
            Blink();
        }

        // ---------------------------------------------------------- ataque

        void StartNextAttack()
        {
            bool continues = Time.time - _lastAttackEnd <= _comboWindow;
            _comboIndex = continues ? (_comboIndex + 1) % _combo.Length : 0;
            Begin(_combo[_comboIndex], Mode.Attack);
            ComboStep?.Invoke(_comboIndex);
        }

        void StartSpecial()
        {
            SpecialCharge = 0;
            _health.SetInvulnerable(true);
            Begin(_special, Mode.Special);
            CombatFeel.Instance?.Special();
            SpecialStarted?.Invoke();
        }

        void Begin(AttackData attack, Mode mode)
        {
            _current = attack;
            _mode = mode;
            _elapsed = 0f;
            _queued = false;
            _windowDone = false;
            _alreadyHit.Clear();
            _motor.Locked = true;

            // Girarse hacia el enemigo más prometedor (o hacia donde empuja el joystick).
            Vector3 aim = _motor.WishDirection.sqrMagnitude > 0.01f ? _motor.WishDirection : transform.forward;
            var target = CombatQuery.FindTarget(transform.position, aim, _targetRange,
                Layers.Mask(Layers.Enemy), Team.Player);
            _target = target?.transform;
            if (target != null) _motor.Face(target.transform.position - transform.position);
            else _motor.Face(aim);

            _animator.SetFloat(ActionSpeed, attack.animSpeed);
            // Desde startTime: el clip entra ya cerca del golpe.
            _animator.CrossFadeInFixedTime(attack.animatorState, 0.05f, 0, attack.startTime);
        }

        void TickAttack(float dt)
        {
            _elapsed += dt;
            var a = _current;
            float clipTime = a.startTime + _elapsed * a.animSpeed;
            float norm = clipTime / a.clipLength;

            // Avance durante la anticipación: el golpe tiene peso. Y si el
            // objetivo está lejos para el golpe, IMÁN: salta hacia él hasta
            // quedar a distancia justa. «Basta con acercarse y machacar» (GDD §5.3).
            if (clipTime < a.impactTime) _motor.Drive(transform.forward * LungeSpeed(a, clipTime));

            bool inWindow = norm >= a.WindowStart && norm <= a.WindowEnd;
            if (inWindow) HitWindow(a);
            else if (norm > a.WindowEnd) _windowDone = true;

            if (_elapsed >= a.Duration)
            {
                if (_mode == Mode.Special) { EndSpecial(); return; }
                bool chain = _queued;
                EndAttack();
                if (chain) StartNextAttack();
            }
            // Tras el impacto se puede encadenar YA si se pulsó: machacar tiene
            // que sentirse inmediato.
            else if (_mode == Mode.Attack && _windowDone && _queued && _elapsed >= a.Windup + a.recovery * 0.5f)
            {
                EndAttack();
                StartNextAttack();
            }
        }

        float LungeSpeed(AttackData a, float clipTime)
        {
            if (_target == null || _mode == Mode.Special) return a.lunge;
            Vector3 to = _target.position - transform.position;
            to.y = 0;
            float gap = to.magnitude - a.reach; // lo que falta para que quede en el centro del golpe
            float timeLeft = Mathf.Max((a.impactTime - clipTime) / a.animSpeed, 0.02f);
            return Mathf.Clamp(gap / timeLeft, a.lunge, _magnetMaxSpeed);
        }

        void HitWindow(AttackData a)
        {
            Vector3 chest = transform.position + Vector3.up * 1.1f;
            Vector3 center = _mode == Mode.Special
                ? transform.position + Vector3.up * a.height
                : transform.position + transform.forward * a.reach + Vector3.up * a.height;
            float radius = _mode == Mode.Special ? _specialRadius : a.radius;

            CombatQuery.Overlap(center, radius, Layers.Mask(Layers.Enemy), Team.Player, chest, _hits);
            foreach (var target in _hits)
            {
                if (_alreadyHit.Contains(target)) continue;
                _alreadyHit.Add(target);
                Vector3 dir = target.transform.position - transform.position;
                dir.y = 0;
                var hit = new HitInfo
                {
                    attack = a,
                    source = gameObject,
                    sourceTeam = Team.Player,
                    direction = dir.sqrMagnitude > 1e-4f ? dir.normalized : transform.forward,
                    point = target.transform.position + Vector3.up * 0.7f,
                };
                if (!target.TakeHit(hit)) continue;
                CombatFeel.Instance?.Hit(hit);
                // Solo el combo carga el especial (trampa de Godot: si el especial
                // se cargaba a sí mismo, era encadenable casi sin límite).
                if (_mode == Mode.Attack)
                    SpecialCharge = Mathf.Min(_specialCost, SpecialCharge + a.specialCharge);
            }
        }

        void EndAttack()
        {
            _mode = Mode.Free;
            _motor.Locked = false;
            _lastAttackEnd = Time.time;
            _animator.CrossFadeInFixedTime("Locomotion", 0.12f);
        }

        void EndSpecial()
        {
            _health.SetInvulnerable(false);
            _comboIndex = -1;
            EndAttack();
            SpecialEnded?.Invoke();
        }

        // ---------------------------------------------------------- recibir

        public bool TakeHit(in HitInfo hit)
        {
            if (!CanBeHit) return false;
            return _health.Apply(hit);
        }

        void OnDamaged(HitInfo hit, int left)
        {
            if (_mode == Mode.Special) EndSpecial();
            CombatFeel.Instance?.Hurt(hit);
            if (hit.attack != null)
                _motor.Knock(hit.direction * hit.attack.knockback + Vector3.up * hit.attack.knockbackUp);
            if (left <= 0) return; // lo remata OnDepleted
            _mode = Mode.Hurt;
            _modeUntil = Time.time + _hurtTime;
            _motor.Locked = true;
            _animator.SetFloat(ActionSpeed, 1f);
            _animator.CrossFadeInFixedTime("Hurt", 0.05f);
        }

        void OnDepleted(HitInfo hit)
        {
            // Sin game over (GDD pilar 1): se marea y vuelve con la vida llena.
            _mode = Mode.Dizzy;
            _modeUntil = Time.time + _dizzyTime;
            _motor.Locked = true;
            _animator.SetFloat(ActionSpeed, 1f);
            _animator.CrossFadeInFixedTime("Dizzy", 0.1f);
        }

        void Recover()
        {
            if (_mode == Mode.Dizzy) _health.Refill();
            _mode = Mode.Free;
            _motor.Locked = false;
            _animator.CrossFadeInFixedTime("Locomotion", 0.15f);
        }

        /// Parpadeo mientras dura la invulnerabilidad tras un golpe (no durante
        /// el especial: ahí es invulnerable pero no está herida).
        void Blink()
        {
            bool blinking = _health.IsInvulnerable && _mode != Mode.Special && _mode != Mode.Dizzy;
            bool visible = !blinking || Mathf.Repeat(Time.time, 0.16f) < 0.1f;
            foreach (var r in _renderers)
                if (r != null && r.enabled != visible) r.enabled = visible;
        }

        /// Para pruebas y para el modo desarrollador.
        public void FillSpecial() => SpecialCharge = _specialCost;

        void OnDrawGizmosSelected()
        {
            if (_combo == null || _combo.Length == 0 || _combo[0] == null) return;
            var a = _combo[Mathf.Clamp(_comboIndex, 0, _combo.Length - 1)] ?? _combo[0];
            Gizmos.color = new Color(1f, 0.4f, 0.7f, 0.5f);
            Gizmos.DrawWireSphere(transform.position + transform.forward * a.reach + Vector3.up * a.height, a.radius);
        }
    }
}
