using UnityEngine;
using UnityEngine.InputSystem;

namespace SailorMoon.Player
{
    /// <summary>
    /// Lo que la jugadora pide, venga de donde venga: teclado y ratón, mando o
    /// pantalla táctil.
    ///
    /// Todo el juego lee de aquí, nunca de una tecla ni de un toque directo.
    /// En Godot, mandar una pulsación de tecla desde fuera no movía a la
    /// jugadora porque ella leía acciones (trampa 27); aquí la acción es la
    /// única puerta y los controles táctiles escriben en ella igual que el
    /// teclado.
    ///
    /// Controles de PC (los mismos que muestra docs/CONTROLES.md):
    ///   WASD / flechas   mover            Shift        andar
    ///   Ratón            cámara (clic en el juego para capturarlo, Esc para soltarlo)
    ///   Rueda            acercar / alejar la cámara
    ///   Espacio          saltar           J / clic     atacar
    ///   K                especial
    ///   R                volver al inicio F1           panel de rendimiento
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class InputReader : MonoBehaviour
    {
        [Tooltip("Grados por píxel de ratón o de dedo.")]
        [SerializeField] float _lookSensitivity = 0.12f;
        [Tooltip("Grados por segundo con el stick derecho del mando.")]
        [SerializeField] float _stickLookSpeed = 160f;
        [Tooltip("Empuje del joystick al andar con Shift (por debajo del umbral de andar del motor).")]
        [SerializeField, Range(0.1f, 0.54f)] float _walkPush = 0.45f;
        [Tooltip("Capturar el ratón al hacer clic en el juego (en PC).")]
        [SerializeField] bool _lockCursorOnClick = true;

        InputAction _move, _walk, _mouseLook, _dragLook, _stickLook, _zoom, _jump, _attack, _special, _respawn;

        /// Dirección de movimiento pedida, en la pantalla: x derecha, y adelante.
        public Vector2 Move { get; private set; }
        /// Giro de cámara pedido en este fotograma, en grados (x yaw, y pitch).
        public Vector2 LookDelta { get; private set; }
        /// Acercar (+) o alejar (−) la cámara en este fotograma, en pasos.
        public float Zoom { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool SpecialPressed { get; private set; }
        public bool RespawnPressed { get; private set; }

        // Lo que escriben los controles táctiles (TouchControls).
        Vector2 _touchMove, _touchLook;
        bool _touchJump, _touchAttack, _touchSpecial;
        public void SetTouchMove(Vector2 v) => _touchMove = v;
        public void AddTouchLook(Vector2 pixels) => _touchLook += pixels;
        public void PressTouchJump() => _touchJump = true;
        public void PressTouchAttack() => _touchAttack = true;
        public void PressTouchSpecial() => _touchSpecial = true;

        static bool CursorCaptured => Cursor.lockState == CursorLockMode.Locked;

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

            _walk = new InputAction("Walk", InputActionType.Button);
            _walk.AddBinding("<Keyboard>/leftShift");
            _walk.AddBinding("<Keyboard>/rightShift");

            // Ratón capturado: se mueve y gira la cámara, como en cualquier
            // juego de PC. Sin capturar: arrastrando con el botón derecho.
            _mouseLook = new InputAction("MouseLook", InputActionType.Value, "<Mouse>/delta");
            _dragLook = new InputAction("DragLook", InputActionType.Value);
            _dragLook.AddCompositeBinding("OneModifier")
                .With("Modifier", "<Mouse>/rightButton")
                .With("Binding", "<Mouse>/delta");
            _stickLook = new InputAction("StickLook", InputActionType.Value, "<Gamepad>/rightStick");

            _zoom = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");

            _jump = new InputAction("Jump", InputActionType.Button);
            _jump.AddBinding("<Keyboard>/space");
            _jump.AddBinding("<Gamepad>/buttonSouth");

            _attack = new InputAction("Attack", InputActionType.Button);
            _attack.AddBinding("<Keyboard>/j");
            _attack.AddBinding("<Mouse>/leftButton");
            _attack.AddBinding("<Gamepad>/buttonWest");

            _special = new InputAction("Special", InputActionType.Button);
            _special.AddBinding("<Keyboard>/k");
            _special.AddBinding("<Gamepad>/buttonNorth");

            _respawn = new InputAction("Respawn", InputActionType.Button);
            _respawn.AddBinding("<Keyboard>/r");
            _respawn.AddBinding("<Gamepad>/select");
        }

        InputAction[] All => new[] { _move, _walk, _mouseLook, _dragLook, _stickLook, _zoom, _jump, _attack, _special, _respawn };

        void OnEnable() { foreach (var a in All) a.Enable(); }
        void OnDisable() { foreach (var a in All) a.Disable(); }
        void OnDestroy() { foreach (var a in All) a.Dispose(); }

        // Se lee antes que nadie (orden de ejecución) para que todo el
        // fotograma vea la misma entrada.
        void Update()
        {
            HandleCursor(out bool clickUsedToCapture);

            Vector2 move = _move.ReadValue<Vector2>();
            if (_walk.IsPressed() && move.sqrMagnitude > 0.01f)
                move = move.normalized * _walkPush;
            Move = Vector2.ClampMagnitude(move + _touchMove, 1f);

            Vector2 mouse = CursorCaptured ? _mouseLook.ReadValue<Vector2>() : _dragLook.ReadValue<Vector2>();
            Vector2 look = (mouse + _touchLook) * _lookSensitivity;
            look += _stickLook.ReadValue<Vector2>() * (_stickLookSpeed * Time.unscaledDeltaTime);
            LookDelta = look;
            _touchLook = Vector2.zero;

            // Un «clic» de rueda vale 120 en Windows y 1 en Mac: solo importa el signo.
            float scroll = _zoom.ReadValue<float>();
            Zoom = Mathf.Abs(scroll) > 0.01f ? Mathf.Sign(scroll) : 0f;

            JumpPressed = _jump.WasPressedThisFrame() || _touchJump;
            AttackPressed = (_attack.WasPressedThisFrame() && !clickUsedToCapture) || _touchAttack;
            SpecialPressed = _special.WasPressedThisFrame() || _touchSpecial;
            RespawnPressed = _respawn.WasPressedThisFrame();
            _touchJump = _touchAttack = _touchSpecial = false;
        }

        /// El primer clic en el juego captura el ratón (y no cuenta como
        /// ataque); Esc lo suelta. En el móvil no hay ratón y no hace nada.
        void HandleCursor(out bool clickUsedToCapture)
        {
            clickUsedToCapture = false;
            if (!_lockCursorOnClick || Mouse.current == null || Application.isMobilePlatform) return;

            if (CursorCaptured && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!CursorCaptured && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                clickUsedToCapture = true;
            }
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused && CursorCaptured)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
