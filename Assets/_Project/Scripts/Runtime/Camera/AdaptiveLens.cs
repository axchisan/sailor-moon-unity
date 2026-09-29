using Unity.Cinemachine;
using UnityEngine;

namespace SailorMoon.CameraRig
{
    /// <summary>
    /// Ajusta la lente de la cámara a la forma de la pantalla.
    ///
    /// POR QUÉ EXISTE: Unity fija el campo de visión VERTICAL, y el horizontal
    /// sale de la forma de la pantalla. Con 55° verticales, en un 16:9 se ven
    /// 86° de lado a lado; en el teléfono de medida (1610×720, 20:9) se veían
    /// **99°**: un gran angular que aleja todo, estira los bordes y hace que el
    /// mundo parezca pequeño (docs/ESCALA.md §4).
    ///
    /// Aquí se decide el HORIZONTAL (lo que la jugadora ve alrededor) y el
    /// vertical se calcula, dentro de unos límites para que ni un teléfono
    /// alargado se vuelva un túnel ni una tableta cuadrada un ojo de pez.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    [ExecuteAlways]
    public class AdaptiveLens : MonoBehaviour
    {
        [Tooltip("Grados que se ven de lado a lado. 75° es lo de un juego de acción en tercera persona.")]
        [SerializeField, Range(50, 100)] float _horizontalFov = 75f;
        [Tooltip("Límites del campo vertical resultante, en grados.")]
        [SerializeField] Vector2 _verticalLimits = new(38f, 52f);
        [Tooltip("Plano cercano en metros. Más de 0,1 da más precisión de profundidad (menos parpadeo entre superficies).")]
        [SerializeField] float _near = 0.2f;
        [Tooltip("Hasta dónde se dibuja, en metros. Lo de más lejos lo cubre la niebla.")]
        [SerializeField] float _far = 400f;

        CinemachineCamera _cam;
        float _lastAspect;

        void OnEnable()
        {
            _cam = GetComponent<CinemachineCamera>();
            _lastAspect = 0;
        }

        void Update()
        {
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            if (Mathf.Approximately(aspect, _lastAspect)) return;
            _lastAspect = aspect;

            var lens = _cam.Lens;
            lens.FieldOfView = VerticalFor(aspect);
            lens.NearClipPlane = _near;
            lens.FarClipPlane = _far;
            _cam.Lens = lens;
        }

        /// Campo vertical que da el horizontal buscado en esta forma de pantalla.
        public float VerticalFor(float aspect)
        {
            float halfH = _horizontalFov * 0.5f * Mathf.Deg2Rad;
            float v = 2f * Mathf.Atan(Mathf.Tan(halfH) / aspect) * Mathf.Rad2Deg;
            return Mathf.Clamp(v, _verticalLimits.x, _verticalLimits.y);
        }
    }
}
