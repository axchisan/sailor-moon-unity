using UnityEngine;

namespace SailorMoon.Combat
{
    /// <summary>
    /// Un golpe: cuánto hace, cómo empuja, cómo se siente y cuándo conecta.
    /// Se edita en el Inspector (≈ un archivo de configuración): ajustar el
    /// combate es tocar estos números, no el código.
    ///
    /// Los valores de partida vienen del combate de Godot, ya probado con la
    /// jugadora (docs/diseno/05-COMBATE.md).
    ///
    /// EL TIEMPO LO MARCA LA ANIMACIÓN: el golpe conecta en el instante en que
    /// el puño o el pie llegan más lejos en el clip (`impactTime`, medido con
    /// «Sailor Moon ▸ Combate ▸ Medir impactos»). `animSpeed` acelera el clip;
    /// la anticipación que se ve es impactTime / animSpeed.
    /// </summary>
    [CreateAssetMenu(menuName = "Sailor Moon/Combate/Ataque", fileName = "Attack")]
    public class AttackData : ScriptableObject
    {
        public string displayName = "Golpe";

        [Header("Animación")]
        [Tooltip("Estado del Animator que se reproduce.")]
        public string animatorState = "Attack1";
        [Tooltip("Clip del que se mide el impacto (Art/Animations/Humanoid).")]
        public string clipName = "attack_1";
        [Tooltip("Velocidad de reproducción del clip. Por encima de 3,5 se ve borroso.")]
        [Range(0.5f, 4f)] public float animSpeed = 2f;
        [Tooltip("Segundos DEL CLIP en que el golpe llega a su punto más lejano. Lo mide la herramienta; no tocar a mano.")]
        public float impactTime = 0.3f;
        [Tooltip("Duración del clip en segundos (medida).")]
        public float clipLength = 1f;
        [Tooltip("Anticipación que se quiere ver, en segundos reales. El clip empieza ya avanzado para conseguirla (muchos clips de Mixamo tardan más de un segundo en llegar al golpe).")]
        [Range(0.05f, 1f)] public float targetWindup = 0.14f;
        [Tooltip("Segundo del clip desde el que se reproduce. Lo calcula la herramienta a partir del impacto y la anticipación.")]
        public float startTime;
        [Tooltip("Medio ancho de la ventana de golpe, en segundos reales.")]
        [Range(0.02f, 0.3f)] public float activeHalfWidth = 0.06f;
        [Tooltip("Segundos reales tras el impacto hasta poder moverse o encadenar.")]
        [Range(0.05f, 1.5f)] public float recovery = 0.14f;

        [Header("Alcance")]
        [Tooltip("Radio de la zona de golpe, en metros.")]
        public float radius = 0.9f;
        [Tooltip("Distancia del centro de la zona por delante del personaje.")]
        public float reach = 1.0f;
        [Tooltip("Altura del centro de la zona.")]
        public float height = 0.9f;

        [Header("Daño e impacto")]
        public int damage = 1;
        [Tooltip("Empuje horizontal al que recibe, en m/s.")]
        public float knockback = 3f;
        [Tooltip("Empuje hacia arriba, en m/s.")]
        public float knockbackUp = 0f;
        [Tooltip("Avance del que golpea durante la anticipación, en m/s. Da peso.")]
        public float lunge = 2.2f;
        [Tooltip("Rompe el escudo del Cofrecito.")]
        public bool breaksShield;
        [Tooltip("Cuánta carga de especial da al conectar (el propio especial no da nada).")]
        public int specialCharge = 1;

        [Header("Sensación")]
        [Tooltip("Congelado al conectar, en segundos. Lo que más cambia cómo se siente.")]
        [Range(0f, 0.3f)] public float hitStop = 0.045f;
        [Tooltip("Fuerza de la sacudida de cámara.")]
        [Range(0f, 2f)] public float shake = 0.15f;
        [Tooltip("Golpe fuerte: otro sonido y más partículas.")]
        public bool heavy;

        /// Segundos reales desde que empieza hasta que conecta.
        public float Windup => (impactTime - startTime) / animSpeed;
        /// Segundos reales que dura el golpe hasta poder encadenar.
        public float Duration => Windup + recovery;
        /// Tiempo normalizado del clip (0–1) en que se abre y cierra la ventana.
        public float WindowStart => Mathf.Clamp01((impactTime - activeHalfWidth * animSpeed) / clipLength);
        public float WindowEnd => Mathf.Clamp01((impactTime + activeHalfWidth * animSpeed) / clipLength);
    }
}
