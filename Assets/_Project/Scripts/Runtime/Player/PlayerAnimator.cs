using UnityEngine;

namespace SailorMoon.Player
{
    /// <summary>
    /// Pasa el estado del movimiento al Animator. Nada más: qué animación
    /// suena en cada caso lo decide el Animator Controller, que se edita en
    /// su ventana y no aquí.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int Grounded = Animator.StringToHash("Grounded");
        static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        static readonly int Jump = Animator.StringToHash("Jump");
        static readonly int LocomotionRate = Animator.StringToHash("LocomotionRate");

        /// Velocidad a la que la animación de correr no patina (medida sobre
        /// el clip, docs/ESCALA.md §6). Por encima, se reproduce más rápido.
        public const float RunClipSpeed = 3.9f;

        [SerializeField] Animator _animator;
        [Tooltip("Suavizado del parámetro de velocidad, para que el cambio andar/correr no dé tirones.")]
        [SerializeField] float _speedDamp = 0.08f;

        PlayerMotor _motor;

        void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            // La altura del cuerpo la pone la física, no la animación: con la
            // raíz sin aplicar se descarta lo que el clip suba o baje entero
            // (ver ModelImportRules.OnPreprocessAnimation).
            _animator.applyRootMotion = false;
            _motor.Jumped += () => _animator.SetTrigger(Jump);
        }

        void LateUpdate()
        {
            // En m/s, no en fracción: los umbrales de la mezcla son las
            // velocidades naturales de cada clip, así los pies no patinan.
            float speed = _motor.PlanarSpeed;
            _animator.SetFloat(Speed, speed, _speedDamp, Time.deltaTime);
            _animator.SetFloat(LocomotionRate, Mathf.Max(1f, speed / RunClipSpeed));
            _animator.SetBool(Grounded, _motor.IsGrounded);
            _animator.SetFloat(VerticalSpeed, _motor.VerticalSpeed);
        }
    }
}
