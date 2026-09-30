using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace SailorMoon.Combat
{
    /// <summary>
    /// Todo el «jugo» del combate en un solo sitio: la parada de impacto, la
    /// sacudida de cámara, las chispas, el sonido y el destello.
    ///
    /// GDD pilar 2: machacar un botón tiene que sentirse increíble; un beat 'em
    /// up es un 20 % de sistemas y un 80 % de sensación. Se puede apagar entero
    /// (`JuiceEnabled`) para comparar, que es como se afinó en Godot.
    ///
    /// Va UNO por escena (lo pone el montador de la escena). Para la cámara hace
    /// falta un CinemachineImpulseListener en la CinemachineCamera.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CombatFeel : MonoBehaviour
    {
        public static CombatFeel Instance { get; private set; }

        [SerializeField] ParticleSystem _sparksPrefab;
        [SerializeField] int _sparksPool = 10;

        [Header("Sonido")]
        [SerializeField] AudioClip _hit;
        [SerializeField] AudioClip _hitHeavy;
        [SerializeField] AudioClip _hurt;
        [SerializeField] AudioClip _defeat;
        [SerializeField] AudioClip _special;
        [SerializeField] AudioClip _block;
        [Tooltip("Variación de tono ±, para que no canse (GDD: ±10 %).")]
        [SerializeField, Range(0, 0.3f)] float _pitchJitter = 0.1f;
        [SerializeField] int _voices = 8;

        [Header("Parada de impacto")]
        [Tooltip("Velocidad del tiempo durante la parada (casi quieto, no parado del todo).")]
        [SerializeField, Range(0f, 0.3f)] float _stopScale = 0.05f;

        public bool JuiceEnabled { get; set; } = true;

        CinemachineImpulseSource _impulse;
        ParticleSystem[] _sparks;
        int _nextSpark;
        AudioSource[] _sources;
        int _nextSource;
        float _stopUntilRealtime;
        Coroutine _stopRoutine;

        void Awake()
        {
            Instance = this;
            _impulse = GetComponent<CinemachineImpulseSource>();

            if (_sparksPrefab != null)
            {
                _sparks = new ParticleSystem[_sparksPool];
                for (int i = 0; i < _sparksPool; i++)
                    _sparks[i] = Instantiate(_sparksPrefab, transform);
            }
            _sources = new AudioSource[_voices];
            for (int i = 0; i < _voices; i++)
            {
                _sources[i] = gameObject.AddComponent<AudioSource>();
                _sources[i].playOnAwake = false;
                _sources[i].spatialBlend = 0f; // el combate se oye siempre igual de cerca
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        /// Todo lo que pasa cuando un golpe conecta.
        public void Hit(in HitInfo hit)
        {
            var a = hit.attack;
            if (a == null) return;
            Play(a.heavy ? _hitHeavy : _hit);
            if (!JuiceEnabled) return;
            HitStop(a.hitStop);
            Shake(a.shake);
            Sparks(hit.point, a.heavy ? 2 : 1);
        }

        public void Hurt(in HitInfo hit)
        {
            Play(_hurt);
            if (!JuiceEnabled) return;
            HitStop(hit.attack != null ? hit.attack.hitStop : 0.06f);
            Shake(hit.attack != null ? hit.attack.shake : 0.3f);
        }

        public void Defeat(Vector3 point)
        {
            Play(_defeat);
            if (JuiceEnabled) Sparks(point, 3);
        }

        public void Blocked(Vector3 point)
        {
            Play(_block);
            if (JuiceEnabled) Shake(0.1f);
        }

        public void Special() => Play(_special, jitter: false);

        /// Congela casi del todo durante `seconds` de tiempo REAL. Si llega otra
        /// parada antes de acabar, se alarga en vez de apilarse.
        public void HitStop(float seconds)
        {
            if (seconds <= 0f) return;
            _stopUntilRealtime = Mathf.Max(_stopUntilRealtime, Time.unscaledTime + seconds);
            _stopRoutine ??= StartCoroutine(StopRoutine());
        }

        IEnumerator StopRoutine()
        {
            Time.timeScale = _stopScale;
            // Tiempo real: con el tiempo del juego casi parado, un temporizador
            // normal no acabaría nunca (trampa 3 de Godot).
            while (Time.unscaledTime < _stopUntilRealtime) yield return null;
            Time.timeScale = 1f;
            _stopRoutine = null;
        }

        public void Shake(float strength)
        {
            if (strength > 0f && _impulse != null) _impulse.GenerateImpulseWithForce(strength);
        }

        void Sparks(Vector3 point, int bursts)
        {
            if (_sparks == null) return;
            for (int i = 0; i < bursts; i++)
            {
                var ps = _sparks[_nextSpark];
                _nextSpark = (_nextSpark + 1) % _sparks.Length;
                ps.transform.position = point;
                ps.Play(true);
            }
        }

        void Play(AudioClip clip, bool jitter = true)
        {
            if (clip == null) return;
            var src = _sources[_nextSource];
            _nextSource = (_nextSource + 1) % _sources.Length;
            src.pitch = jitter ? 1f + Random.Range(-_pitchJitter, _pitchJitter) : 1f;
            src.PlayOneShot(clip);
        }
    }
}
