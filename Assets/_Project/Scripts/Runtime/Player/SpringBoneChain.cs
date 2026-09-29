using System.Collections.Generic;
using UnityEngine;

namespace SailorMoon.Player
{
    /// <summary>
    /// Física de muelle para una cadena de huesos: coletas, lazos, colas.
    ///
    /// Mixamo riguea solo el cuerpo; las coletas de Serena (95 cm, hasta las
    /// rodillas) llevan cinco huesos propios por lado, añadidos en Blender
    /// (docs/diseno/09-FISICA-PELO.md). Las animaciones no los tocan: la
    /// animación mueve el cuerpo y esto mueve el pelo encima.
    ///
    /// Es el algoritmo de SpringBone de VRM, simplificado. Corre en
    /// LateUpdate, DESPUÉS del Animator, y escribe la rotación de los huesos
    /// directamente. (En Godot esto necesitó un SkeletonModifier3D porque el
    /// AnimationTree pisaba cualquier otra escritura, trampa 89; en Unity el
    /// orden de LateUpdate basta.)
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class SpringBoneChain : MonoBehaviour
    {
        [Tooltip("Primer hueso de la cadena (p. ej. Hair_L_1). Los siguientes se toman como su primer hijo.")]
        [SerializeField] Transform _root;
        [SerializeField, Range(1, 12)] int _length = 5;

        [Header("Física")]
        [Tooltip("Fuerza con la que vuelve a su postura. Más alto = más rígido.")]
        [SerializeField, Range(0, 4)] float _stiffness = 1.4f;
        [Tooltip("Rozamiento. Alto = se para antes; bajo = se columpia mucho.")]
        [SerializeField, Range(0, 1)] float _drag = 0.45f;
        [Tooltip("Cuánto tira la gravedad.")]
        [SerializeField, Range(0, 4)] float _gravity = 0.9f;
        [Tooltip("Separación máxima respecto a la postura animada. Evita que se doble del revés.")]
        [SerializeField, Range(0, 180)] float _maxAngle = 75f;

        struct Joint
        {
            public Transform bone;
            public Quaternion restLocal;   // rotación local de reposo
            public Vector3 axis;           // hacia el hijo, en local
            public float length;
            public Vector3 tail, prevTail; // punta en mundo
        }

        readonly List<Joint> _joints = new();

        void Start() => Build();

        void Build()
        {
            _joints.Clear();
            Transform t = _root;
            for (int i = 0; i < _length && t != null; i++)
            {
                // El último hueso no tiene hijo: se inventa una punta en su
                // misma dirección para que también se mueva.
                Transform child = t.childCount > 0 ? t.GetChild(0) : null;
                Vector3 tailLocal = child ? child.localPosition : t.localPosition.normalized * 0.1f;
                if (tailLocal.sqrMagnitude < 1e-8f) tailLocal = Vector3.up * 0.1f;
                Vector3 tail = t.TransformPoint(tailLocal);
                _joints.Add(new Joint
                {
                    bone = t,
                    restLocal = t.localRotation,
                    axis = tailLocal.normalized,
                    length = Vector3.Distance(t.position, tail),
                    tail = tail,
                    prevTail = tail,
                });
                t = child;
            }
        }

        void LateUpdate()
        {
            float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
            if (dt <= 0) return;

            for (int i = 0; i < _joints.Count; i++)
            {
                var j = _joints[i];
                Transform parent = j.bone.parent;
                Quaternion parentRot = parent ? parent.rotation : Quaternion.identity;
                // Dónde quiere estar la punta según la animación.
                Quaternion animated = parentRot * j.restLocal;
                Vector3 head = j.bone.position;

                Vector3 inertia = (j.tail - j.prevTail) * (1f - _drag);
                Vector3 spring = animated * j.axis * (_stiffness * dt);
                Vector3 fall = Vector3.down * (_gravity * dt);
                Vector3 next = j.tail + inertia + spring + fall;

                // Longitud fija: el pelo no se estira.
                next = head + (next - head).normalized * j.length;

                // Límite de ángulo respecto a la postura animada.
                Vector3 want = animated * j.axis;
                Vector3 got = (next - head).normalized;
                float angle = Vector3.Angle(want, got);
                if (angle > _maxAngle)
                {
                    got = Vector3.Slerp(want, got, _maxAngle / angle);
                    next = head + got * j.length;
                }

                j.prevTail = j.tail;
                j.tail = next;
                j.bone.rotation = Quaternion.FromToRotation(want, got) * animated;
                _joints[i] = j;
            }
        }

        void OnDisable()
        {
            // Al reactivarse, que no salga disparado desde donde se quedó.
            for (int i = 0; i < _joints.Count; i++)
            {
                var j = _joints[i];
                j.tail = j.prevTail = j.bone.position + j.bone.rotation * j.axis * j.length;
                _joints[i] = j;
            }
        }
    }
}
