# Rendimiento

> **Regla uno: si la medida no viene del teléfono, no es una medida.** El Mac va
> sobrado con todo y no distingue nada (medido en Godot: 180 fps con el juego
> entero, 156 sin dibujar nada).

## El aparato de referencia

HONOR LGN-NX3 · Android 16 · Snapdragon con **Adreno 619** · pantalla
**1610×720** · gama media-baja de 2022. Si va fino ahí, va fino en el de la niña.

## Presupuestos

Objetivo 60 fps; 30 estables es aceptable antes que 45 con tirones.

| Recurso | Presupuesto |
|---|---|
| Tiempo de GPU por fotograma | **< 14 ms** (deja margen bajo los 16,7 del vsync) |
| Tiempo de CPU por fotograma | < 10 ms |
| Fotogramas por encima de 20 ms | < 1 % |
| Triángulos (pantalla + sombra) | < 150.000 |
| Batches (draw calls) | < 150 |
| Personaje principal | 25.000 tris · textura 2048 |
| Enemigo | 5.000 tris · textura 1024 |
| Luces | 1 direccional (sombra) + 2 puntuales por vértice |

## Cómo se mide

### A mano: el panel

**F1** en el Mac, **tres dedos** en el teléfono. Muestra:

- fps de media, **p99** del tiempo de fotograma y % de fotogramas por encima de
  20 ms. La media sola esconde los tirones.
- **CPU y GPU en ms** (`FrameTimingManager`). Los fps topan en 60 por el vsync
  —que en Android no se puede quitar— y ahí dejan de decir nada; el tiempo de
  GPU sigue diciendo cuánto margen queda. *(En Godot había que subir la
  resolución como «lupa» para ver algo; aquí no hace falta.)*
- batches, set-pass, triángulos y sombra (solo en la compilación de desarrollo).
- **Estado térmico** de Android y temperatura de la batería: el teléfono pierde
  hasta un 22 % caliente, más que casi cualquier optimización.

### Automático: el banco

La escena de prueba (`Scenes/Sandbox/PerfTest.unity`) lleva un `PerfBenchmark`
que clava la cámara en el «punto de medida» —el equivalente al sitio de
arranque de Godot: media pantalla de suelo, un tercio de cielo y arbolado—,
congela a la jugadora y mide. Primero la referencia, luego una cosa apagada
cada vez, **con la referencia medida otra vez entre prueba y prueba** para
descontar la deriva térmica.

```bash
ADB=/opt/homebrew/share/android-commandlinetools/platform-tools/adb
$ADB install -r export/sailor_moon_unity_dev.apk
$ADB logcat -c
# el nombre de la actividad lo da: $ADB shell cmd package resolve-activity --brief com.familia.sailormoon.unity
$ADB shell am start -n com.familia.sailormoon.unity/com.unity3d.player.UnityPlayerGameActivity --es bench 1
$ADB logcat -s Unity | grep BENCH
```

Una línea por fila (logcat corta los mensajes largos).

### Profundo: el Profiler de Unity

Con la compilación de desarrollo instalada: *Window ▸ Analysis ▸ Profiler*,
elegir el teléfono en la lista de destinos. Da el reparto por función de CPU,
el Frame Debugger (qué se dibuja y en qué orden) y memoria. Es lo que en Godot
hubo que construir a mano.

## Lo que ya está decidido (y por qué)

Todo en `ProjectSetup.cs`, con el motivo al lado.

| Ajuste | Valor | Motivo medido en Godot |
|---|---|---|
| Resolución de render | 1,0 | Bajarla emborrona el contorno; lo rechazó el usuario. FSR queda por probar |
| HDR | No | El cel shading no lo usa; doble ancho de banda |
| Antialiasing | MSAA 4x | FXAA fue lo más caro medido (relee el fotograma) |
| Textura de profundidad / opaca | No | Obligan a copiar el fotograma a medias |
| Sombra | 1 cascada, 30 m, 2048, suave baja | El pase de sombra llegó a costar el triple que la imagen |
| Luz estática | Horneada (Subtractive) | Iluminar por píxel el suelo era el cuello |
| Ambiente | Tres colores, por vértice | Las dos muestras de cubemap por píxel costaban 25 % |
| Reflejos | Ninguno | Muestra de cubemap por píxel que el toon no usa |
| Niebla | Lineal de URP (por vértice) | Una sola capa atmosférica |
| Cielo | Degradado propio | El de Godot llegó a 32 senos por píxel |
| Frame pacing | Swappy activado | Menos tirones con vsync |

## Resultados

### Prueba 1 — ¿Unity rinde más que Godot en el mismo teléfono?

*Pendiente de medir en el teléfono.* Referencia de Godot en el sitio de
arranque: **35-44 fps** antes de la cuarta pasada y **57,7 fps jugando** después
de quitarle las muestras de cubemap al terreno, con picos del 4,6 % por encima
de 20 ms.

| Fila | fps | p99 ms | GPU ms | CPU ms | batches | térmico |
|---|---|---|---|---|---|---|
| referencia | | | | | | |
| sin terreno | | | | | | |
| sin decorado | | | | | | |
| sin sombras | | | | | | |
| sin personaje | | | | | | |
