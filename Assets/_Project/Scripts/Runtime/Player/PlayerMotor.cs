using UnityEngine;

namespace SailorMoon.Player
{
    /// <summary>
    /// Cómo se mueve Serena: andar relativo a la cámara, gravedad y salto.
    ///
    /// Solo física y números. No sabe de animaciones (PlayerAnimator las lee
    /// de aquí) ni de dónde viene la entrada (InputReader).
    ///
    /// Los valores por defecto vienen del ajuste que se hizo jugando en Godot
    /// (docs/diseno/05-COMBATE.md): coyote time y jump buffer generosos,
    /// porque a los 8 años el ritmo no es fiable y el salto tiene que salir.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Andar")]
        [Tooltip("m/s. La animación de correr da 3,9 m/s de zancada; por encima se acelera en proporción (docs/ESCALA.md §6).")]
        [SerializeField] float _runSpeed = 4.8f;
        [Tooltip("Por debajo de este empuje del joystick, anda en vez de correr.")]
        [SerializeField, Range(0, 1)] float _walkThreshold = 0.55f;
        [Tooltip("m/s. La animación de andar da 1,5 m/s: a esa velocidad los pies no patinan.")]
        [SerializeField] float _walkSpeed = 1.5f;
        [SerializeField] float _acceleration = 40f;
        [Tooltip("Grados por segundo al girar hacia donde va.")]
        [SerializeField] float _turnSpeed = 720f;

        [Header("Salto")]
        [SerializeField] float _jumpHeight = 1.6f;
        [SerializeField] float _gravity = -24f;
        [Tooltip("Se cae más rápido que se sube: el salto se siente con peso.")]
        [SerializeField] float _fallMultiplier = 1.5f;
        [Tooltip("Segundos tras dejar el suelo en los que aún se puede saltar.")]
        [SerializeField] float _coyoteTime = 0.15f;
        [Tooltip("Segundos que se recuerda un salto pulsado antes de tocar suelo.")]
        [SerializeField] float _jumpBuffer = 0.2f;

        [SerializeField] Transform _cameraTransform;
        [Tooltip("Por debajo de esta altura se considera que se ha caído del mundo y vuelve al inicio.")]
        [SerializeField] float _killHeight = -30f;

        CharacterController _controller;
        InputReader _input;
        Vector3 _planarVelocity;
        float _verticalSpeed;
        float _lastGroundedTime = -10f;
        float _lastJumpPressTime = -10f;

        public bool IsGrounded { get; private set; }
        public float VerticalSpeed => _verticalSpeed;
        /// 0 quieta, 0,5 andando, 1 corriendo: lo que lee la animación.
        public float Speed01 => _planarVelocity.magnitude / _runSpeed;
        /// Velocidad horizontal real en m/s: lo que lee la animación.
        public float PlanarSpeed => _planarVelocity.magnitude;
        /// Se dispara el fotograma en que despega.
        public event System.Action Jumped;
        public event System.Action Landed;

        Vector3 _spawnPosition;
        Quaternion _spawnRotation;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<InputReader>();
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        /// Vuelve al punto de salida. Lo pide la jugadora (R) o salta solo si
        /// cae del mundo: nunca debe quedarse atascada sin salida (lección de
        /// Godot, trampas 95 y 104).
        public void Respawn()
        {
            // El CharacterController pisa cualquier posición escrita mientras
            // está activo: se apaga, se mueve y se enciende.
            _controller.enabled = false;
            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            _controller.enabled = true;
            _planarVelocity = Vector3.zero;
            _verticalSpeed = 0f;
        }

        void Update()
        {
            if (_input.RespawnPressed || transform.position.y < _killHeight)
            {
                Respawn();
                return;
            }
            float dt = Time.deltaTime;
            bool wasGrounded = IsGrounded;
            IsGrounded = _controller.isGrounded;
            if (IsGrounded) _lastGroundedTime = Time.time;
            if (IsGrounded && !wasGrounded) Landed?.Invoke();

            MovePlanar(dt);
            MoveVertical(dt);

            Vector3 motion = (_planarVelocity + Vector3.up * _verticalSpeed) * dt;
            _controller.Move(motion);
        }

        void MovePlanar(float dt)
        {
            Vector2 stick = _input.Move;
            Vector3 wish = Vector3.zero;
            if (stick.sqrMagnitude > 0.0001f)
            {
                // Relativo a la cámara, aplanado: arriba en el joystick es
                // «hacia donde mira la cámara», no hacia el norte del mundo.
                Vector3 fwd = _cameraTransform ? _cameraTransform.forward : Vector3.forward;
                fwd.y = 0;
                fwd.Normalize();
                Vector3 right = new(fwd.z, 0, -fwd.x);
                Vector3 dir = (fwd * stick.y + right * stick.x);
                float push = stick.magnitude;
                float speed = push < _walkThreshold
                    ? Mathf.Lerp(0, _walkSpeed, push / _walkThreshold)
                    : Mathf.Lerp(_walkSpeed, _runSpeed, (push - _walkThreshold) / (1 - _walkThreshold));
                wish = dir.normalized * speed;

                Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * dt);
            }
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, wish, _acceleration * dt);
        }

        void MoveVertical(float dt)
        {
            if (_input.JumpPressed) _lastJumpPressTime = Time.time;

            bool canJump = Time.time - _lastGroundedTime <= _coyoteTime;
            bool wantsJump = Time.time - _lastJumpPressTime <= _jumpBuffer;
            if (canJump && wantsJump)
            {
                _verticalSpeed = Mathf.Sqrt(2f * _jumpHeight * -_gravity);
                _lastGroundedTime = _lastJumpPressTime = -10f;
                Jumped?.Invoke();
                return;
            }

            if (IsGrounded && _verticalSpeed < 0)
            {
                // Un poco hacia abajo siempre, para que baje cuestas pegada al
                // suelo en vez de ir dando saltitos.
                _verticalSpeed = -2f;
                return;
            }
            float g = _verticalSpeed < 0 ? _gravity * _fallMultiplier : _gravity;
            _verticalSpeed += g * dt;
        }
    }
}
