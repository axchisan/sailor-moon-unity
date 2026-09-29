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

        CinemachineOrbitalFollow _orbit;

        void Awake() => _orbit = GetComponent<CinemachineOrbitalFollow>();

        void LateUpdate()
        {
            if (_input == null) return;
            Vector2 d = _input.LookDelta;
            if (d == Vector2.zero) return;

            _orbit.HorizontalAxis.Value = _orbit.HorizontalAxis.ClampValue(_orbit.HorizontalAxis.Value + d.x);
            float dy = _invertY ? d.y : -d.y;
            _orbit.VerticalAxis.Value = _orbit.VerticalAxis.ClampValue(_orbit.VerticalAxis.Value + dy);
        }
    }
}
