using UnityEngine;

namespace SailorMoon.Core
{
    /// <summary>
    /// Lo que tiene que estar puesto antes de la primera escena, venga de
    /// donde venga el juego (menú, escena de prueba, el editor).
    /// </summary>
    static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // En Android Unity limita a 30 fps si nadie pide otra cosa. Sin
            // esta línea el juego iría a la mitad y parecería un problema de
            // rendimiento que no existe.
            Application.targetFrameRate = 60;

            // Que la pantalla no se apague mientras se juega con el mando o
            // se mira una cinemática sin tocar.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
