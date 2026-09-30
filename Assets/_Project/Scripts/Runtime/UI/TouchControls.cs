using SailorMoon.Player;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SailorMoon.UI
{
    /// <summary>
    /// Controles de pantalla: joystick flotante a la izquierda, arrastrar para
    /// mirar a la derecha y botones.
    ///
    /// El joystick aparece donde se pone el dedo (GDD §7): a los 8 años es
    /// mejor que uno fijo, que obliga a acertar en un círculo sin mirar.
    /// Escribe en InputReader, igual que el teclado: el resto del juego no
    /// sabe si se juega con los dedos o con un mando.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class TouchControls : MonoBehaviour
    {
        [SerializeField] InputReader _input;
        [SerializeField] RectTransform _stickBase;
        [SerializeField] RectTransform _stickKnob;
        [SerializeField] RectTransform _jumpButton;
        [SerializeField] RectTransform _attackButton;
        [SerializeField] RectTransform _specialButton;
        [Tooltip("Radio del joystick en píxeles de referencia (1280×720).")]
        [SerializeField] float _stickRadius = 110f;
        [Tooltip("Mostrar los botones aunque no haya pantalla táctil (para probarlos en el editor).")]
        [SerializeField] bool _showOnDesktop;

        Canvas _canvas;
        int _stickFinger = -1, _lookFinger = -1;
        Vector2 _stickOrigin;

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            _canvas = GetComponentInParent<Canvas>();
            SetStickVisible(false);
            // En PC los botones táctiles solo estorban: se juega con teclado,
            // ratón o mando.
            bool touch = Application.isMobilePlatform || _showOnDesktop;
            if (_jumpButton) _jumpButton.gameObject.SetActive(touch);
            if (_attackButton) _attackButton.gameObject.SetActive(touch);
            if (_specialButton) _specialButton.gameObject.SetActive(touch);
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            _input?.SetTouchMove(Vector2.zero);
        }

        void Update()
        {
            if (_input == null) return;
            float scale = _canvas ? _canvas.scaleFactor : 1f;

            foreach (var touch in Touch.activeTouches)
            {
                int id = touch.finger.index;
                Vector2 pos = touch.screenPosition;

                if (touch.began)
                {
                    if (Hit(_jumpButton, pos)) { _input.PressTouchJump(); continue; }
                    if (Hit(_attackButton, pos)) { _input.PressTouchAttack(); continue; }
                    if (Hit(_specialButton, pos)) { _input.PressTouchSpecial(); continue; }
                    if (pos.x < Screen.width * 0.5f && _stickFinger < 0)
                    {
                        _stickFinger = id;
                        _stickOrigin = pos;
                        SetStickVisible(true);
                        _stickBase.position = pos;
                        _stickKnob.position = pos;
                    }
                    else if (pos.x >= Screen.width * 0.5f && _lookFinger < 0)
                    {
                        _lookFinger = id;
                    }
                }

                if (id == _stickFinger)
                {
                    Vector2 offset = Vector2.ClampMagnitude(pos - _stickOrigin, _stickRadius * scale);
                    _stickKnob.position = _stickOrigin + offset;
                    _input.SetTouchMove(offset / (_stickRadius * scale));
                    if (touch.ended) ReleaseStick();
                }
                else if (id == _lookFinger)
                {
                    // Píxeles de referencia, no físicos: que girar la cámara
                    // cueste el mismo gesto en cualquier pantalla.
                    _input.AddTouchLook(touch.delta / scale);
                    if (touch.ended) _lookFinger = -1;
                }
            }
        }

        void ReleaseStick()
        {
            _stickFinger = -1;
            _input.SetTouchMove(Vector2.zero);
            SetStickVisible(false);
        }

        void SetStickVisible(bool visible)
        {
            if (_stickBase) _stickBase.gameObject.SetActive(visible);
        }

        static bool Hit(RectTransform rect, Vector2 screenPos) =>
            rect != null && rect.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, null);
    }
}
