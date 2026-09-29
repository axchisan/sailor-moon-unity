using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SailorMoon.Diagnostics
{
    /// <summary>
    /// Banco de medición automático, para comparar con los mismos criterios
    /// con los que se midió Godot.
    ///
    /// Clava la cámara en un punto fijo (<see cref="_view"/>), congela a la
    /// jugadora y mide una tanda de pruebas, cada una apagando UNA cosa. La
    /// referencia se vuelve a medir entre prueba y prueba: el teléfono no
    /// rinde igual dos minutos seguidos y así la deriva térmica se ve y se
    /// descuenta (trampa 122 de Godot).
    ///
    /// Se lanza desde el Mac con el teléfono enchufado:
    ///   adb shell am start -n com.familia.sailormoon.unity/com.unity3d.player.UnityPlayerGameActivity --es bench 1
    /// y el resultado sale por logcat, una línea por fila (logcat corta los
    /// mensajes largos, trampa 129):  adb logcat -s Unity | grep BENCH
    /// </summary>
    public class PerfBenchmark : MonoBehaviour
    {
        [Tooltip("Dónde se clava la cámara para medir.")]
        [SerializeField] Transform _view;
        [SerializeField] Camera _camera;
        [Tooltip("Lo que se desactiva mientras se mide (cerebro de Cinemachine, jugadora, controles).")]
        [SerializeField] Behaviour[] _freeze;

        [Header("Qué se puede apagar")]
        [SerializeField] UnityEngine.Terrain _terrain;
        [SerializeField] GameObject _decor;
        [SerializeField] Light _sun;
        [SerializeField] GameObject _player;

        [Header("Tiempos")]
        [SerializeField] float _settleSeconds = 2f;
        [SerializeField] float _measureSeconds = 8f;
        [Tooltip("Lanzarlo al darle a Play en el editor (para probarlo sin teléfono).")]
        [SerializeField] bool _runInEditor;

        readonly List<string> _log = new();

        void Start()
        {
            if (ShouldRun()) StartCoroutine(Run());
        }

        bool ShouldRun()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent = activity.Call<AndroidJavaObject>("getIntent");
                return !string.IsNullOrEmpty(intent.Call<string>("getStringExtra", "bench"));
            }
            catch (Exception) { return false; }
#else
            return _runInEditor;
#endif
        }

        IEnumerator Run()
        {
            foreach (var b in _freeze) if (b) b.enabled = false;
            _camera.transform.SetPositionAndRotation(_view.position, _view.rotation);

            var tests = new (string name, Action off, Action on)[]
            {
                ("sin_terreno", () => _terrain.enabled = false, () => _terrain.enabled = true),
                ("sin_decorado", () => _decor.SetActive(false), () => _decor.SetActive(true)),
                ("sin_sombras", () => _sun.shadows = LightShadows.None, () => _sun.shadows = LightShadows.Soft),
                ("sin_personaje", () => _player.SetActive(false), () => _player.SetActive(true)),
            };

            Print($"[BENCH] inicio {SystemInfo.deviceModel} · {SystemInfo.graphicsDeviceName} · {SystemInfo.graphicsDeviceType} · {Screen.width}x{Screen.height}");
            // Primera pasada larga para que compile shaders y se asiente.
            yield return new WaitForSecondsRealtime(5f);

            using var stats = new FrameStats(2000);
            var refs = new List<float>();
            refs.Add(0);
            yield return Measure("referencia", stats, r => refs[^1] = r);
            foreach (var (name, off, on) in tests)
            {
                off();
                float result = 0;
                yield return Measure(name, stats, r => result = r);
                on();
                float before = refs[^1];
                refs.Add(0);
                yield return Measure("referencia", stats, r => refs[^1] = r);
                float refAvg = (before + refs[^1]) / 2f;
                Print($"[BENCH] {name}: GPU {result:0.00} ms vs ref {refAvg:0.00} ms → {(refAvg / result - 1f) * 100f:+0;-0}%");
            }
            Print($"[BENCH] deriva de la referencia: {(refs[^1] / refs[0] - 1f) * 100f:+0.0;-0.0}%");
            Print("[BENCH] fin");
            foreach (var b in _freeze) if (b) b.enabled = true;
        }

        IEnumerator Measure(string name, FrameStats stats, Action<float> gpuResult)
        {
            yield return new WaitForSecondsRealtime(_settleSeconds);
            stats.Reset();
            float end = Time.realtimeSinceStartup + _measureSeconds;
            while (Time.realtimeSinceStartup < end)
            {
                stats.Tick();
                yield return null;
            }
            var (thermal, battery) = FrameStats.Thermal();
            Print($"[BENCH] {name,-14} {stats.AverageFps,5:0.0} fps · p50 {stats.PercentileMs(0.5f):0.0} · p99 {stats.PercentileMs(0.99f):0.0} ms · " +
                  $">20ms {stats.FractionAbove(20f) * 100f:0.0}% · CPU {stats.CpuMsAverage:0.00} · GPU {stats.GpuMsAverage:0.00} ms · " +
                  $"batches {stats.Batches} · tris {stats.Triangles} · térmico {thermal} · {battery:0.0}°C");
            gpuResult(stats.GpuMsAverage);
        }

        void Print(string line)
        {
            _log.Add(line);
            Debug.Log(line);
        }
    }
}
