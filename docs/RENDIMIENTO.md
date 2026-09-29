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

### Prueba 1 — 2026-09-29, HONOR frío (térmico 0–1, batería 38–40 °C)

Compilación de desarrollo, escena `PerfTest`, punto de medida fijo. La deriva
de la referencia entre la primera y la última medida fue **+0,1 %**: las filas
son comparables.

| Fila | fps | p99 ms | >20 ms | GPU ms | CPU ms | tris |
|---|---|---|---|---|---|---|
| referencia (media de 4) | 55–60 | 16,7–33 | 0–8 % | **16,6–17,2** | 16,7–18,1 | 159k |
| sin terreno | 60 | — | — | 10,5 | — | — |
| sin decorado | 59,7 | 16,7 | 0,2 % | 12,5 | 16,7 | 85k |
| sin sombras | 59,9 | 16,7 | 0 % | 14,1 | 16,7 | 134k |
| sin personaje | 59,9 | 16,7 | 0 % | 13,7 | 16,7 | 84k |

**Arranque en frío** (abrir la app → escena lista): **4,1 s**, dos veces. En
Godot eran 15 s del toque en JUGAR al nivel listo (no es la misma medida: aquí
todavía no hay enemigos ni interfaz completa).

**Qué dice:**

- **Va a 60 fps en el sitio canónico, pero sin margen**: la GPU gasta 16,6 ms
  de los 16,7 disponibles, y en dos de las cuatro referencias hay un 5–8 % de
  fotogramas por encima de 20 ms. Godot daba 57,7 fps en su sitio de arranque:
  Unity está **al nivel o algo mejor, no muy por encima**. La diferencia la
  tiene que dar el horneado, que aquí todavía no llega al terreno.
- **El terreno es lo más caro: 6,1 ms (37 %)**, igual que en Godot. Confirma
  que la primera tarea de rendimiento es el **shader toon de terreno con luz
  horneada** (U5).
- **El decorado cuesta 4,4 ms** con ~1.600 props: revisar distancia de
  dibujado de lo menudo y la oclusión horneada.
- **Serena sola cuesta 3,1 ms y 75.000 triángulos**: sus 25k se dibujan tres
  veces (imagen, contorno, sombra) y ocupa mucha pantalla de cerca. Candidatos:
  contorno por nivel de detalle y un LOD de 12k para cuando está lejos.
- **Las sombras, 3 ms**: bajar el alcance a 20–25 m o la resolución a 1024.
- **Triángulos por encima del presupuesto** (159k frente a 150k).
- Los contadores de batches salen a 0 en el teléfono (Vulkan): medir draw
  calls con el Frame Debugger conectado.

**Pendiente de medir en el banco:** MSAA 4x frente a nada, y FSR a 0,85.

### Prueba 2 — 2026-09-30, tras la corrección de escala (docs/ESCALA.md)

Mismo sitio y compilación de desarrollo. Teléfono frío (térmico 0, batería
30–31 °C). Deriva de la referencia: −1,0 %.

| Fila | GPU ms | Ganancia | tris |
|---|---|---|---|
| referencia (media de 5) | **15,9** (antes 16,6–17,2) | — | 147k (antes 159k) |
| sin terreno | 10,1 | +59 % | 140k |
| sin decorado | 13,1 | +21 % | 84k |
| sin sombras | 12,4 | +28 % | 122k |
| sin personaje | 12,3 | +29 % | 72k |

- **0 % de fotogramas por encima de 20 ms en las cinco referencias** (antes, hasta
  un 8 %). La cámara nueva (75° horizontales en vez de 99°) abarca menos mundo.
- El decorado baja de 4,4 a 2,8 ms aunque los árboles sean el doble de grandes:
  hay 140 cerezos en vez de 200 y se ven menos a la vez.
- **El terreno sigue siendo el 37 %** (5,9 ms): el shader toon con luz horneada
  sigue siendo lo primero de la Fase 1.
- Serena pesa algo más (3,5 ms): con la cámara a 5 m ocupa más pantalla.
