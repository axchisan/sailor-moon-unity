using System;
using Unity.Profiling;
using UnityEngine;

namespace SailorMoon.Diagnostics
{
    /// <summary>
    /// Números del fotograma que sirven para decidir, no para mirar.
    ///
    /// Lo aprendido midiendo en Godot (docs/aprendido/RENDIMIENTO.md):
    /// · La media esconde los tirones: 57 fps de media con un pico cada dos
    ///   segundos se juega peor que 45 clavados. Por eso hay percentiles.
    /// · Los fps topan en 60 por el vsync y dejan de decir nada; el tiempo de
    ///   GPU no. Si la GPU tarda 9 ms de 16,7 hay margen aunque ponga «60».
    /// · Si los draw calls cambian entre dos medidas, la cámara se movió y la
    ///   comparación no vale.
    /// · El teléfono se estrangula por calor: el estado térmico va al lado.
    /// </summary>
    public sealed class FrameStats : IDisposable
    {
        readonly float[] _frameMs;
        int _count, _head;
        readonly FrameTiming[] _timing = new FrameTiming[1];
        float _cpuMs, _gpuMs;
        double _cpuSum, _gpuSum;
        int _timingCount;

        ProfilerRecorder _batches, _setPass, _triangles, _shadowCasters;

        public FrameStats(int window = 600)
        {
            _frameMs = new float[window];
            // Solo existen en las compilaciones de desarrollo; en la final
            // salen a 0 y el panel lo dice.
            _batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            _setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            _shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
        }

        public void Dispose()
        {
            _batches.Dispose(); _setPass.Dispose(); _triangles.Dispose(); _shadowCasters.Dispose();
        }

        public void Reset()
        {
            _count = 0; _head = 0;
            _cpuSum = _gpuSum = 0; _timingCount = 0;
        }

        /// Llamar una vez por fotograma.
        public void Tick()
        {
            _frameMs[_head] = Time.unscaledDeltaTime * 1000f;
            _head = (_head + 1) % _frameMs.Length;
            _count = Mathf.Min(_count + 1, _frameMs.Length);

            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, _timing) > 0)
            {
                // Suavizado exponencial: el dato suelto de un fotograma baila mucho.
                _cpuMs = Mathf.Lerp(_cpuMs, (float)_timing[0].cpuFrameTime, 0.05f);
                _gpuMs = Mathf.Lerp(_gpuMs, (float)_timing[0].gpuFrameTime, 0.05f);
                _cpuSum += _timing[0].cpuFrameTime;
                _gpuSum += _timing[0].gpuFrameTime;
                _timingCount++;
            }
        }

        public int Samples => _count;
        /// Suavizados, para el panel.
        public float CpuMs => _cpuMs;
        public float GpuMs => _gpuMs;
        /// Media exacta desde el último Reset, para el banco.
        public float CpuMsAverage => _timingCount > 0 ? (float)(_cpuSum / _timingCount) : 0;
        public float GpuMsAverage => _timingCount > 0 ? (float)(_gpuSum / _timingCount) : 0;
        public long Batches => _batches.Valid ? _batches.LastValue : 0;
        public long SetPassCalls => _setPass.Valid ? _setPass.LastValue : 0;
        public long Triangles => _triangles.Valid ? _triangles.LastValue : 0;
        public long ShadowCasters => _shadowCasters.Valid ? _shadowCasters.LastValue : 0;

        public float AverageFps
        {
            get
            {
                if (_count == 0) return 0;
                double sum = 0;
                for (int i = 0; i < _count; i++) sum += _frameMs[i];
                return (float)(1000.0 * _count / sum);
            }
        }

        /// Tiempo de fotograma en el percentil p (0–1), en ms.
        public float PercentileMs(float p)
        {
            if (_count == 0) return 0;
            var tmp = new float[_count];
            Array.Copy(_frameMs, tmp, _count);
            Array.Sort(tmp);
            return tmp[Mathf.Clamp(Mathf.CeilToInt(p * _count) - 1, 0, _count - 1)];
        }

        /// Fracción de fotogramas por encima de <paramref name="ms"/>.
        public float FractionAbove(float ms)
        {
            if (_count == 0) return 0;
            int n = 0;
            for (int i = 0; i < _count; i++) if (_frameMs[i] > ms) n++;
            return (float)n / _count;
        }

        /// Estado térmico de Android (0 = normal … 6 = apagado) y temperatura
        /// de la batería en °C. En el editor, (-1, 0).
        public static (int status, float batteryC) Thermal()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var power = activity.Call<AndroidJavaObject>("getSystemService", "power");
                int status = power.Call<int>("getCurrentThermalStatus");
                using var filter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");
                using var battery = activity.Call<AndroidJavaObject>("registerReceiver", null, filter);
                float temp = battery.Call<int>("getIntExtra", "temperature", 0) / 10f;
                return (status, temp);
            }
            catch (Exception) { return (-1, 0); }
#else
            return (-1, 0);
#endif
        }
    }
}
