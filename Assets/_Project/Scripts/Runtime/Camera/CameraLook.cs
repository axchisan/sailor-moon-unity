using SailorMoon.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace SailorMoon.CameraRig
{
    /// <summary>
    /// Gira la cámara orbital con lo que pida la jugadora (ratón, stick o
    /// dedo). La órbita, la colisión con paredes y el encuadre los hace
    /// Cinemachine; esto solo mueve sus dos ejes.
    ///
    /// En Godot la cámara era un SpringArm3D escrito a mano y estuvo rota toda
    /// la Fase 0 porque el brazo pisaba la posición de sus hijos (trampa 1).
    /// Aquí no se escribe ninguna posición: solo ángulos.
    /// </summary>
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public class CameraLook : MonoBehaviour
    {
        [SerializeField] InputReader _input;
        [Tooltip("Invertir el eje vertical.")]
        [SerializeField] bool _invertY;
        [Tooltip("Distancia mínima y máxima de la cámara (rueda del ratón).")]
        [SerializeField] Vector2 _radiusRange = new(3f, 9f);
        [Tooltip("Metros por paso de rueda.")]
        [SerializeField] float _zoomStep = 0.6f;

        CinemachineOrbitalFollow _orbit;

        void Awake() => _orbit = GetComponent<CinemachineOrbitalFollow>();

        void LateUpdate()
        {
            if (_input == null) return;
            if (_input.Zoom != 0f)
                _orbit.Radius = Mathf.Clamp(_orbit.Radius - _input.Zoom * _zoomStep, _radiusRange.x, _radiusRange.y);

            Vector2 d = _input.LookDelta;
            if (d == Vector2.zero) return;

            _orbit.HorizontalAxis.Value = _orbit.HorizontalAxis.ClampValue(_orbit.HorizontalAxis.Value + d.x);
            float dy = _invertY ? d.y : -d.y;
            _orbit.VerticalAxis.Value = _orbit.VerticalAxis.ClampValue(_orbit.VerticalAxis.Value + dy);
        }
    }
}
