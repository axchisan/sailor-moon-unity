> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/06-GUIA-ASSETS-3D.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Guía de recursos 3D — pipeline definitivo

> **Actualizado el 2026-08-04.** Sustituye a las versiones anteriores basadas en
> VRoid Studio y en Tripo AI. Este documento describe el pipeline que **ya está
> funcionando y verificado de punta a punta**.

## 0. Por qué cambiamos de herramienta (dos veces)

| Herramienta | Por qué se descartó |
|---|---|
| **VRoid Studio** | No trae complementos para construir los trajes de Sailor Moon |
| **Meshy** | El plan gratuito ni siquiera deja descargar modelos de la comunidad |
| **Tripo AI** | Genera gratis, pero **cobra por exportar**. Un modelo que no puedes descargar no sirve |

**La trampa a reconocer:** casi todas estas plataformas te dejan *generar* gratis
para engancharte, y ponen el muro de pago en la **exportación**. Al evaluar una
herramienta nueva, lo primero que hay que probar es descargar un archivo.

### La que usamos: [hi3d.ai](https://www.hi3d.ai)

- ✅ Exporta gratis en GLB
- ✅ Calidad excelente (ver resultados abajo)
- ⚠️ Solo imagen → 3D, sin modo texto
- ⚠️ **No deja configurar polígonos ni resolución de textura**

Esa última limitación es la clave de todo lo que sigue: hi3d suelta mallas de
**~2.000.000 de triángulos con texturas 4K**. Por eso **el paso por Blender ya no
es opcional, es obligatorio**.

---

## 1. Resultados reales (2026-08-04)

Los tres modelos procesados con `tools/blender_preparar_modelo.py`:

| Modelo | Triángulos | Textura | Altura | Tamaño |
|---|---|---|---|---|
| **Serena (Sailor Moon)** | 1.999.714 → **25.000** | 4096 → 2048 | 1,55 m | 78 MB → **7,0 MB** |
| **Peluchín** | 1.999.312 → **5.000** | 4096 → 1024 | 0,90 m | 73 MB → **1,6 MB** |
| **Cofrecito** | 1.998.638 → **5.000** | 4096 → 1024 | 0,80 m | 80 MB → **2,3 MB** |

**231 MB → 10,9 MB.** Reducción del 98,75% de geometría **sin pérdida visible**:
cara, odangos, lazo, cuello marinero y guantes intactos. La textura carga el
detalle que la malla ya no tiene.

Verificado en Godot: los tres se importan, renderizan y reciben cel shading
MToon a 60 fps.

---

## 2. El proceso, paso a paso

### 2.1 Generar en hi3d.ai

Sube **una imagen frontal, cuerpo entero, fondo blanco, luz uniforme**. Los
requisitos completos están en §3. Descarga el **GLB** y déjalo en `Models/`.

> `Models/` lleva un archivo `.gdignore`: Godot ignora esa carpeta por completo.
> Sin él, el motor intenta importar los originales de 80 MB y la caché se va a
> **273 MB**. Con él, se queda en 18 MB.

### 2.2 Procesar en Blender

Abre `tools/blender_preparar_modelo.py` en la pestaña *Scripting* de Blender,
ajusta el bloque `CONFIG` de arriba y ejecuta.

Qué hace, y por qué cada paso importa:

1. **Une las mallas** en una sola → menos draw calls
2. **Quita los Empty** que mete el importador glTF
3. **Suelda vértices duplicados** (las mallas de IA traen miles) y arregla normales
4. **Escala a metros reales** y pone el **origen en los pies**
5. **Decima con simetría en X** — en una cara, es la diferencia entre que quede
   bien o que salga un ojo torcido
6. **Baja las texturas**
7. **Exporta GLB** (Godot) y opcionalmente **FBX** (Mixamo)

**Presupuestos** (de [`01-ARQUITECTURA.md`](01-ARQUITECTURA.md) §8):

| Tipo | Triángulos | Textura | Altura típica |
|---|---|---|---|
| Personaje principal | 25.000 | 2048 | 1,55 m |
| Enemigo | 5.000 | 1024 | 0,80–0,90 m |
| Prop grande | 3.000 | 512 | según objeto |
| Prop pequeño | 1.000 | 512 | según objeto |

### 2.3 Importar en Godot

Deja el `.glb` en `assets/models/characters/` (o `props/`, `environment/`).
Godot lo importa solo como `PackedScene`.

### 2.4 Aplicar el cel shading

Los modelos de IA llegan con materiales PBR: se ven **realistas, no anime**.

Cuelga los modelos de un nodo con el script `src/core/toon_root.gd` y listo.
O por código:

```gdscript
ToonMaterial.apply($Model)
```

`src/core/toon_material.gd` reaprovecha el shader **MToon** que trajo el addon
VRM y lo aplica a cualquier malla, conservando la textura y añadiendo el
contorno negro con un `next_pass` en `cull_front` (*inverted hull*).

Parámetros que se pueden tocar desde el inspector en `ToonRoot`:

| Parámetro | Qué hace |
|---|---|
| `shade_color` | Tinte de la sombra. Es lo que crea el escalón de luz del anime |
| `toony` | Cerca de 1 = transición dura luz/sombra. Bajarlo la suaviza |
| `outline_width` | Grosor del contorno **en metros**. 0.006 es fino y limpio |
| `rim_mix` | Brillo del borde. Pasarse deja el personaje lavado |

Escena de prueba: `scenes/prototypes/test_modelos.tscn`.

---

## 3. Imágenes de referencia

hi3d.ai lee la imagen **al detalle**, así que la referencia decide el resultado.

| Requisito | Detalle |
|---|---|
| **Pose** | T-pose o A-pose. Los brazos pegados al cuerpo complican el rigging después (§4) |
| **Cuerpo completo** | Sin recortes. Que se vean los pies |
| **Fondo** | Blanco liso o transparente |
| **Luz** | Uniforme. Las sombras marcadas la IA las toma por geometría |
| **Resolución** | Mínimo 1040×1040 |

### Las mejores fuentes

**1. Fotos de figuras coleccionables** ← *la mejor con diferencia*

Busca `S.H.Figuarts Sailor Moon official photo`, `Bandai Sailor Mars figure
product photo`. Las fotos de producto son exactamente lo que la IA quiere:
cuerpo entero, fondo blanco, luz de estudio, alta resolución, pose neutra.

**2. Hojas de modelo de animación (*settei*)**

`Sailor Moon settei`, `Sailor Moon 設定資料`, `Sailor Moon model sheet`. Son las
hojas que usaban los animadores, con frente, perfil y espalda consistentes.

**3. Generar la referencia con IA**

[3D AI Studio](https://www.3daistudio.com/Tools/CharacterSheetGenerator),
[Anifusion](https://anifusion.ai/dashboard/character-sheets/),
[Pixa](https://www.pixa.com/create/character-turnaround-sheet).

Usamos arte oficial solo como referencia para un regalo privado que no se
publica ni se monetiza, en línea con lo acordado en el GDD.

---

## 4. Animación — dos caminos distintos

Esto es importante y no es obvio: **los enemigos no necesitan esqueleto.**

### 4.1 Serena y las Sailors → Mixamo

Son humanoides y necesitan animación esquelética real.

1. Blender exporta `Models/mixamo/serena_sailor_para_mixamo.fbx` (ya generado)
2. Súbelo a [Mixamo](https://www.mixamo.com/) (gratis con cuenta Adobe)
3. El auto-rigger te pide marcar barbilla, muñecas, codos, rodillas e ingle
4. Descarga la primera animación **With Skin** y las demás **Without Skin**
5. En Godot, el retargeting por mapeo de huesos las aplica sobre el modelo

### A-pose: hecho, y una lección sobre cómo medir

Primero avisé de que Serena tenía "los brazos pegados al cuerpo", basándome en un
ratio ancho/alto de 0,558. **Esa medición estaba mal:** lo que ensancha la silueta
son las coletas, que llegan a ±0,43 m. Al aislar la geometría de los brazos, el
ángulo real era de **32° respecto a la vertical**, que ya es una A-pose válida.

Aun así se abrieron hasta **44°**, el óptimo para el auto-rigger de Mixamo.

**Cómo se hizo, y el error que hay que evitar:** el primer intento rotó los
vértices seleccionados **por islas de malla**, y rasgó los codos. El motivo es que
el brazo atraviesa varias islas, y la frontera entre una que gira y otra que no
es exactamente por donde se abre el corte.

La forma correcta es un **peso continuo por posición**, sin mirar islas:

```
w = radial(distancia_al_eje_del_brazo) × axial(avance_a_lo_largo_del_brazo)
```

- `radial`: 1 dentro de 10,5 cm del eje (el brazo mide 10 cm de radio máximo),
  desvaneciéndose a 0 a los 16,5 cm — así el pelo cercano no se arrastra
- `axial`: 0 en el hombro, subiendo a 1 en el primer 30% del brazo — así la unión
  con el torso no se estira

Al ser una función continua de la posición, dos vértices vecinos reciben pesos
casi idénticos **aunque pertenezcan a islas distintas**. Es imposible que rasgue.

Archivos resultantes:
- `assets/models/characters/serena_sailor.glb` — el del juego, ya en A-pose
- `Models/mixamo/serena_sailor_apose.fbx` — **este es el que sube a Mixamo**
- `Models/serena_apose.blend` — por si hay que retocar

Las 15 animaciones que necesitamos, y dónde buscarlas:

| Categoría | Animaciones | Buscar en Mixamo |
|---|---|---|
| Locomoción | `idle`, `walk`, `run` | *Idle*, *Walking*, *Running* |
| Aire | `jump_start`, `jump_loop`, `land` | *Jump* |
| Combate | `attack_1`, `attack_2`, `attack_3`, `special` | *Martial Arts*, *Fighting*, *Magic* |
| Reacción | `hurt`, `dizzy`, `victory` | *Hit Reaction*, *Dizzy*, *Victory* |
| Narrativa | `transform_pose`, `talk_idle` | *Praying*, *Talking* |

Se preparan **una vez** y sirven para las cinco Sailors, porque comparten
esqueleto. Ésta es la decisión que hace viable el beat 'em up.

### 4.2 Peluchín, Cofrecito y compañía → animación procedural

Son bolas y cofres con patitas. **Mixamo no puede riguearlos, y no hace falta.**

Se animan por código o con un `AnimationPlayer` sobre el nodo entero:

| Estado | Animación procedural |
|---|---|
| Idle | Rebote vertical suave (`sin(t)`) |
| Approach | Rebote más rápido + ligera inclinación hacia delante |
| Telegraph | Encogerse (squash) y vibrar |
| Attack | Estirarse hacia delante (stretch) y volver |
| Stagger | Giro rápido en el aire |
| Purified | Saltitos alegres ← **ya implementado** en `e_purified.gd` |

Es más barato, se ve mejor en criaturas redondas que un esqueleto, y ya tenemos
el patrón funcionando. El *squash & stretch* es el recurso clásico de animación
de dibujos y encaja perfecto con la estética.

---

## 5. Estado actual y qué falta

| Asset | Estado |
|---|---|
| Serena – Sailor Moon | ✅ Procesada e importada. Falta riguear |
| Peluchín | ✅ Procesado e importado. Falta animación procedural |
| Cofrecito | ✅ Procesado e importado. Falta animación procedural |
| Serena – civil | Pendiente de generar |
| Globito | Pendiente de generar |
| Luna y Artemis (gatos) | ✅ Rigueados a mano con [`blender_riguear_gatos.py`](../tools/blender_riguear_gatos.py) |
| Coleccionables | Pendiente (o CC0) |
| **Escenarios** | Ver [`07-ESCENARIOS.md`](07-ESCENARIOS.md) |

**Lo siguiente:** subir el FBX a Mixamo y ver si el auto-rigger traga los brazos
pegados. Es lo único que bloquea sustituir la cápsula por Serena de verdad.
