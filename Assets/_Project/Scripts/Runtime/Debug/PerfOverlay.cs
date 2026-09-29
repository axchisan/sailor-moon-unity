using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SailorMoon.Diagnostics
{
    /// <summary>
    /// Panel de rendimiento: F1, o tres dedos a la vez en el móvil.
    ///
    /// Se refresca cuatro veces por segundo y no en cada fotograma: montar el
    /// texto también cuesta, y un medidor que pesa falsea lo que mide.
    /// </summary>
    public class PerfOverlay : MonoBehaviour
    {
        [SerializeField] Text _label;
        [SerializeField] bool _visibleOnStart = true;

        FrameStats _stats;
        float _nextRefresh;
        bool _threeFingersDown;
        readonly StringBuilder _sb = new(512);

        void OnEnable()
        {
            _stats = new FrameStats();
            EnhancedTouchSupport.Enable();
            if (_label) _label.gameObject.SetActive(_visibleOnStart);
        }

        void OnDisable()
        {
            _stats?.Dispose();
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            _stats.Tick();
            HandleToggle();
            if (_label == null || !_label.gameObject.activeSelf || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.25f;

            var (thermal, battery) = FrameStats.Thermal();
            _sb.Clear();
            _sb.Append($"{_stats.AverageFps:0.0} fps   p99 {_stats.PercentileMs(0.99f):0.0} ms   >20 ms {_stats.FractionAbove(20f) * 100f:0.0}%\n");
            _sb.Append($"CPU {_stats.CpuMs:0.0} ms   GPU {_stats.GpuMs:0.0} ms\n");
            if (_stats.Batches > 0)
                _sb.Append($"batches {_stats.Batches}   setpass {_stats.SetPassCalls}   tris {_stats.Triangles / 1000}k   sombra {_stats.ShadowCasters}\n");
            else
                _sb.Append("draw calls: solo en compilación de desarrollo\n");
            _sb.Append($"{Screen.width}×{Screen.height}");
            if (thermal >= 0) _sb.Append($"   térmico {thermal}   batería {battery:0.0} °C");
            _label.text = _sb.ToString();
        }

        void HandleToggle()
        {
            bool toggle = Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
            bool three = Touch.activeTouches.Count >= 3;
            if (three && !_threeFingersDown) toggle = true;
            _threeFingersDown = three;
            if (toggle && _label) _label.gameObject.SetActive(!_label.gameObject.activeSelf);
        }
    }
}
