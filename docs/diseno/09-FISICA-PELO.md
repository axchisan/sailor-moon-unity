> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/09-FISICA-PELO.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Física de pelo — coletas con inercia

> **Resuelto y verificado el 2026-08-04.**
> Respuesta corta a la pregunta que lo originó: **NO hay que volver a descargar
> ninguna animación de Mixamo.**

## 1. El problema

Mixamo riguea **solo el esqueleto humanoide**: 65 huesos `mixamorig:*` (caderas,
columna, cuello, cabeza, brazos, dedos, piernas). Ni uno para el pelo.

Resultado: las coletas de Serena quedaron pesadas al hueso `Head` y se movían
como un bloque rígido. Con unas coletas de **95 cm que llegan a las rodillas**,
eso se ve fatal en cuanto el personaje se mueve.

Medido sobre el modelo rigueado: **1.867 vértices** pegados a `Head` que están
más de 15 cm por debajo de la cabeza.

## 2. Por qué no hay que rehacer las animaciones

Las animaciones de Mixamo **solo contienen curvas para huesos `mixamorig:*`**.
Los huesos de pelo que añadimos no existen para ellas, así que sencillamente no
los tocan. La animación mueve el cuerpo, y la física mueve el pelo encima.

Las 6 animaciones ya descargadas (`Idle`, `Walking`, `Running`, `Jumping Up`,
`Jumping Down`, `Falling Idle`) **siguen siendo válidas**, y las que falten
también. Se descargan igual que dice [`08-ANIMACIONES-MIXAMO.md`](08-ANIMACIONES-MIXAMO.md).

## 3. Lo que se hizo en Blender

### 3.1 Trazar las coletas

Se aisló la geometría del pelo (vértices dominados por `Head` y laterales) y se
calculó su centroide en franjas de altura, obteniendo el recorrido real de cada
coleta: sale del odango en `z = 0,64` y cae hacia atrás y afuera hasta
`z = −0,27`. Perfectamente simétricas.

### 3.2 Crear las cadenas de huesos

**10 huesos nuevos**, 5 por coleta, siguiendo ese recorrido:

```
mixamorig:Head
├── Hair_L_1 → Hair_L_2 → Hair_L_3 → Hair_L_4 → Hair_L_5
└── Hair_R_1 → Hair_R_2 → Hair_R_3 → Hair_R_4 → Hair_R_5
```

Longitudes: 12, 23, 22, 23 y 20 cm. El primero cuelga de `Head` **sin conectar**
(para no arrastrar la cabeza); los demás encadenados, así al girar uno arrastra
a los siguientes.

El esqueleto pasa de 65 a **75 huesos**.

### 3.3 Repintar los pesos

Para cada vértice de pelo se calcula su posición `u` a lo largo de la cadena y
se reparte el peso entre los dos huesos más cercanos:

| Zona | Peso |
|---|---|
| `u < 0,12` | 100% `Head` — es cuero cabelludo, no debe moverse |
| `0,12 < u < 0,30` | Transición suave `Head` → cadena |
| `u > 0,30` | 100% cadena |

**2.423 vértices repintados**, ninguno se quedó sin peso.

Todo el proceso está guardado en `Models/serena_rigged_pelo.blend`.

## 4. Lo que se hizo en Godot

### `src/core/spring_bone_chain.gd`

Un `SkeletonModifier3D` con el algoritmo clásico de **VRM SpringBone**. Por cada
hueso y cada frame:

```
inercia  = (punta_actual − punta_anterior) × (1 − drag)
elástico = dirección_de_reposo × stiffness × delta
peso     = gravedad × gravity_power × delta
punta_nueva = punta_actual + inercia + elástico + peso
→ se recorta a la longitud del hueso (no se estira)
→ se limita el ángulo (no se dobla del revés)
→ se convierte en rotación local del hueso
```

Coste: 10 huesos con aritmética vectorial simple. Despreciable en móvil.

### `src/core/hair_physics.gd`

Nodo que monta las cadenas en tiempo de ejecución. Se pone como **padre** del
modelo importado; busca el `Skeleton3D` y le cuelga una `SpringBoneChain` por
coleta.

Se hace por código y no en el `.tscn` porque las escenas instanciadas de un
`.glb` no admiten hijos nuevos sin marcarlas como editables, y eso se rompe cada
vez que se reimporta el modelo.

## 5. Parámetros y cómo ajustarlos

Todos en el inspector del nodo `HairPhysics`:

| Parámetro | Defecto | Qué hace |
|---|---|---|
| `stiffness` | 1.4 | Fuerza con la que vuelve al reposo. Más alto = más rígido |
| `drag` | 0.45 | Rozamiento. Alto = se para antes. Bajo = se columpia mucho |
| `gravity_power` | 0.9 | Cuánto pesa. 0 = flota |
| `gravity_dir` | abajo | Apuntarlo de lado simula viento constante |
| `max_angle` | 75° | Tope de separación. Evita que se doble del revés |

**Recetas:**

- *Pelo más vivo y juguetón:* `drag 0.30`, `stiffness 1.0`
- *Pelo más pesado y serio:* `drag 0.65`, `gravity_power 1.6`
- *Ráfaga de viento (transformación, especial):* animar `gravity_dir` y
  `gravity_power` con un `Tween`

**Importante:** llamar a `HairPhysics.reset()` al teletransportar o reaparecer al
personaje. Si no, el pelo sale disparado por la inercia acumulada del salto.

## 6. Verificación

| Prueba | Resultado |
|---|---|
| Cadenas montadas sobre el esqueleto | ✅ `Spring_Hair_L_1`, `Spring_Hair_R_1` |
| Huesos del esqueleto | 75 (65 Mixamo + 10 pelo) |
| Desviación máxima sacudiendo al personaje | **53,3°** |
| Reposo tras frenar | 4,5° (caída natural por gravedad) |
| Viento lateral constante | Coletas horizontales, deformación suave ✅ |
| Rendimiento | 60 fps |

## 7. ⚠️ Trampa que costó tiempo

**Un `SkeletonModifier3D` escribe en un búfer temporal, no en la pose
permanente.** Leer `skeleton.get_bone_pose_rotation()` desde fuera del
modificador devuelve la pose **anterior** al modificador (la de la animación),
no el resultado.

Durante la depuración eso parecía indicar que la física no hacía nada, cuando en
realidad sí funcionaba. Para verificar hay que mirar la malla renderizada o
instrumentar dentro del propio modificador (por eso existe `debug_angles`).

**El otro fallo:** el `swing` calculado está en el espacio del **padre**, así que
se aplica como `swing * rest`, no `rest * swing`. Con el orden invertido el giro
se interpreta en el espacio del propio hueso y el resultado sale casi constante.

## 8. Automatizado para el resto del reparto

Lo que con Serena se hizo a mano está ahora en
**`tools/blender_cadenas_pelo.py`**, y se aplicó a cinco personajes más:

| Personaje | Cadenas | Huesos | Largo |
|---|---|---|---|
| **Serena** | 2 (coletas) | 5 | 95 cm |
| **Sailor Venus** | 3 (izq/centro/der) | 5 | 82 cm |
| **Sailor Mars** | 3 | 5 | 71 cm |
| **Sailor Pluto** | 3 | 5 | 84 cm |
| **Sailor Jupiter** | 1 (coleta alta) | 4 | 26 cm |
| **Tuxedo Mask** | 4 (capa) | 4 | 104 cm |

**Chibi Moon, Mercury, Saturno, Uranus y Neptuno no llevan.** Se comprobó
mirándolos: las coletas de Chibi Moon son cortas y cónicas, y las demás tienen
melena corta. Ponerles cadenas sería gastar huesos para nada.

### Lo difícil no es la física: es encontrar el pelo

Mixamo pesa por cercanía, así que **la melena de Mars acaba pegada a `Hips`** y
se mueve con las caderas. Para repintarla primero hay que saber qué vértices son
pelo, y ninguna pista sirve por sí sola:

| Pista | Por qué no basta |
|---|---|
| Los pesos | Están mal justo en lo que queremos arreglar |
| La posición | La ropa está en medio |
| El color | El naranja del uniforme de Venus y su pelo rubio tienen el **mismo tono** |

Lo que funciona es **crecer una región desde el cuero cabelludo** siguiendo el
color, con dos correcciones que se descubrieron a base de mirar renders:

1. **Comparar el tono, no el color.** La textura trae la iluminación horneada:
   un mismo mechón va de `(0,4 0,3 0,1)` en sombra a `(0,7 0,6 0,4)` en luz. Por
   color absoluto se queda fuera media melena. Normalizando por el canal más
   alto, los dos son el mismo rubio.
2. **Varios tonos de referencia, no uno.** El pelo de Venus tiene mechones
   dorados sobre una masa color crema. Con una sola referencia entra la mitad.

Y aun así hace falta una **barrera**: no propagar a vértices que estén a menos
de 13 cm de algún hueso. El uniforme va ceñido al cuerpo y una melena suelta
cuelga mucho más lejos; sin esa barrera la región saltaba del pelo al traje y de
ahí al modelo entero.

> **Cómo comprobarlo:** pintar la región detectada y renderizar. Y **no usar
> rojo** para la marca: media Sailor va de rojo y no se distingue lo marcado de
> lo que ya era así. Verde chillón.

### Reutilizarlo para otras cosas

El sistema sirve para cualquier cadena de huesos:

- **Faldas** — 3–4 cadenas alrededor de la cintura, `stiffness` alto y
  `max_angle` bajo (30°) para que no se levante demasiado
- **Lazos y cintas** — cadenas cortas de 2–3 huesos, `drag` bajo

Para el Peluchín y el Cofrecito no hace falta: son bolas y cofres, y se animan
procedimentalmente (ver [`06-GUIA-ASSETS-3D.md`](06-GUIA-ASSETS-3D.md) §4.2).

## 9. Montarlo en Godot

Cada personaje tiene una ficha en `assets/data/personajes/*.tres`
(`CharacterData`: modelo, estatura, cadenas, parámetros de física, colores), y
`PersonajeVisual` construye la jerarquía sola:

```
PersonajeVisual
└── ToonRoot        cel shading, girado 180° (los modelos miran a +Z)
    └── HairPhysics las cadenas
        └── modelo  el FBX rigueado
```

> **Trampa:** el modelo se cuelga de `HairPhysics` **antes** de meter la física
> en el árbol. `HairPhysics._ready()` busca el `Skeleton3D` entre sus hijos; si
> entra vacío no monta ninguna cadena y **no avisa de nada**. Por eso
> `tools/vitrina_reparto.gd` cuenta las cadenas vivas y las compara con las que
> pide la ficha: es la única forma de enterarse.

Vitrina de comprobación: `scenes/prototypes/test_reparto.tscn`.

## 10. ⚠️ Pendiente: manchón oscuro detrás de la melena

**Venus y Mars tienen una zona oscura en la parte trasera del pelo, visible solo
desde detrás.** De frente y de tres cuartos se ven perfectas.

No está resuelto. Lo que **ya se ha descartado**, para no repetirlo:

| Sospecha | Cómo se descartó |
|---|---|
| Contorno *inverted hull* | Apagándolo: sigue igual |
| Normales invertidas / culling | Activando *backface culling* en Blender: se ve bien |
| Sombras proyectadas | `shadow_enabled = false`: sigue igual |
| Cel shading | Quitando el material MToon: sigue (en marrón) |
| Transparencia *alpha scissor* | Corregida (§ abajo), pero la mancha sigue |
| La textura | La que usa Godot y la que usa Blender son la misma |
| Precisión del z-buffer | Ajustando `near`/`far`: sigue igual |
| La animación | Parándola: sigue igual |
| La física de pelo | `set_enabled(false)`: sigue igual |
| El color de sombra de la ficha | Poniéndolo neutro y claro: sigue igual |

Lo que sí se arregló por el camino: los materiales llegaban con
`transparency = 2` (**alpha scissor**) porque los modelos traen un canal alpha
que no usan. Blender lo ignora, Godot no, y recortaba píxeles del pelo. Lo
corrige `tools/importar_personaje.gd`, enganchado en el `.import` de cada FBX.

**Siguiente pista a seguir:** en la zona del problema hay **864 pares de caras
casi coincidentes** (separadas entre 0,2 y 6 mm), 195 de ellas con normales
opuestas. Es geometría interior duplicada del generador 3D. Ajustar `near`/`far`
no lo arregló, pero eliminar esas caras interiores sigue siendo lo más probable.
