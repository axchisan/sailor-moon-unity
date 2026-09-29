using UnityEngine;
using UnityEngine.InputSystem;

namespace SailorMoon.Player
{
    /// <summary>
    /// Lo que la jugadora pide, venga de donde venga: teclado, mando o pantalla.
    ///
    /// Todo el juego lee de aquí, nunca de una tecla ni de un toque directo.
    /// En Godot, mandar una pulsación de tecla desde fuera no movía a la
    /// jugadora porque ella leía acciones (trampa 27); aquí la acción es la
    /// única puerta y los controles táctiles escriben en ella igual que el
    /// teclado.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class InputReader : MonoBehaviour
    {
        [Tooltip("Grados por píxel al arrastrar para mirar (ratón y dedo).")]
        [SerializeField] float _lookSensitivity = 0.15f;
        [Tooltip("Grados por segundo con el stick derecho del mando.")]
        [SerializeField] float _stickLookSpeed = 140f;

        InputAction _move, _look, _stickLook, _jump, _attack;

        /// Dirección de movimiento pedida, en la pantalla: x derecha, y adelante.
        public Vector2 Move { get; private set; }
        /// Giro de cámara pedido en este fotograma, en grados (x yaw, y pitch).
        public Vector2 LookDelta { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }

        // Lo que escriben los controles táctiles (TouchControls).
        Vector2 _touchMove, _touchLook;
        bool _touchJump, _touchAttack;
        public void SetTouchMove(Vector2 v) => _touchMove = v;
        public void AddTouchLook(Vector2 pixels) => _touchLook += pixels;
        public void PressTouchJump() => _touchJump = true;
        public void PressTouchAttack() => _touchAttack = true;

        void Awake()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");

            // Ratón: solo con el botón derecho pulsado, para que el cursor
            // siga sirviendo para los menús.
            _look = new InputAction("Look", InputActionType.Value);
            _look.AddCompositeBinding("OneModifier")
                .With("Modifier", "<Mouse>/rightButton")
                .With("Binding", "<Mouse>/delta");
            _stickLook = new InputAction("StickLook", InputActionType.Value, "<Gamepad>/rightStick");

            _jump = new InputAction("Jump", InputActionType.Button);
            _jump.AddBinding("<Keyboard>/space");
            _jump.AddBinding("<Gamepad>/buttonSouth");

            _attack = new InputAction("Attack", InputActionType.Button);
            _attack.AddBinding("<Keyboard>/j");
            _attack.AddBinding("<Mouse>/leftButton");
            _attack.AddBinding("<Gamepad>/buttonWest");
        }

        void OnEnable()
        {
            _move.Enable(); _look.Enable(); _stickLook.Enable(); _jump.Enable(); _attack.Enable();
        }

        void OnDisable()
        {
            _move.Disable(); _look.Disable(); _stickLook.Disable(); _jump.Disable(); _attack.Disable();
        }

        void OnDestroy()
        {
            _move.Dispose(); _look.Dispose(); _stickLook.Dispose(); _jump.Dispose(); _attack.Dispose();
        }

        // Se lee antes que nadie (orden de ejecución) para que todo el
        // fotograma vea la misma entrada.
        void Update()
        {
            Vector2 move = _move.ReadValue<Vector2>() + _touchMove;
            Move = Vector2.ClampMagnitude(move, 1f);

            Vector2 look = (_look.ReadValue<Vector2>() + _touchLook) * _lookSensitivity;
            look += _stickLook.ReadValue<Vector2>() * (_stickLookSpeed * Time.unscaledDeltaTime);
            LookDelta = look;
            _touchLook = Vector2.zero;

            JumpPressed = _jump.WasPressedThisFrame() || _touchJump;
            AttackPressed = _attack.WasPressedThisFrame() || _touchAttack;
            _touchJump = _touchAttack = false;
        }
    }
}

