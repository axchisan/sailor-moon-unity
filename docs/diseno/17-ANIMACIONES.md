> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/17-ANIMACIONES.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Animaciones que faltan — qué bajar de Mixamo

> Ampliación de [`08-ANIMACIONES-MIXAMO.md`](08-ANIMACIONES-MIXAMO.md), que
> explica el proceso. Esto es **la lista de la compra** para las mecánicas
> nuevas del [plan maestro](15-PLAN-MAESTRO.md).

## EL ARCHIVO: `serena_rigged.fbx`

```
/Users/mac/Documents/Dev/Games/sailor-moon/Models/rigged_originales/serena_rigged.fbx
```

**Ese, y solo ese.** Ya está rigueado (bajó así de Mixamo), así que **no hay que
volver a riguear nada**: se sube, Mixamo reconoce el esqueleto, y ya se pueden
descargar animaciones. Si sigues teniéndolo en *My Assets* de tu cuenta, ni
siquiera hace falta subirlo — se elige de ahí y listo.

### Por qué precisamente ese

No es porque sea la protagonista. Es porque **su esqueleto es el más completo de
los once**, y lo he comprobado hueso a hueso:

| Personaje | Huesos | Huesos que Serena no tenga |
|---|---|---|
| **Serena** | **65** | — |
| Mars, Jupiter, Venus, Neptuno, Pluto | 65 | 0 |
| Chibi Moon, Uranus, Tuxedo | 41 | 0 |
| Mercury, Saturno | 33 | 0 |

Los once se rigearon con distinto nivel de detalle en las manos: Serena tiene
los dedos completos (cuatro falanges por dedo) y Mercury no tiene dedos. Pero
**ninguno tiene un hueso que Serena no tenga**: todos los esqueletos son un
subconjunto del suyo.

Eso significa que una animación bajada con Serena:

- Funciona en los once, porque las pistas de huesos que no existen se ignoran
- **Anima los dedos** en los que sí los tienen

Y al revés no: bajándolas con Mercury, a Serena se le quedarían las manos
rígidas para siempre. De ahí que importe cuál subes.

### Resumiendo

1. Subes **`serena_rigged.fbx`** (o lo eliges de *My Assets*)
2. **No lo rigueas** — ya lo está
3. Descargas de él **todas** las animaciones de las tablas de abajo
4. Sirven para los once personajes, ahora y en el futuro

El proyecto ya funciona así: hay una única librería compartida
(`serena_animaciones.res`) que usan Serena, Mercury y las que vengan.

## Ajustes de descarga

Los mismos de siempre, sin cambiarlos:

- **Skin:** *Without Skin* — solo el movimiento, sin el modelo. Son archivos
  pequeños y es lo que las hace válidas para cualquier personaje
- **Formato:** FBX Binary
- **FPS:** 30
- **Keyframe Reduction:** none
- **In Place:** activado en TODAS

Sin *In Place*, la animación arrastra al personaje por el suelo y pelea con el
movimiento del juego.

## Dónde dejarlas

En `assets/animations/serena/`, con **exactamente** el nombre de la tabla. El
nombre es lo que las conecta con el código; si no coincide, no se enteran.

---

## Tanda 1 — Salto y doble salto (prioritaria)

El salto actual se ve raro porque es una sola animación haciendo tres trabajos.
Se parte en piezas:

| Nombre del archivo | Qué buscar en Mixamo | Para qué |
|---|---|---|
| `jump_start.fbx` | `jump` → el impulso, solo la subida | Sustituye la actual. Corta, la salida del suelo |
| `jump_loop.fbx` | `falling idle` | Se repite mientras cae |
| `land.fbx` | `falling to landing` o `hard landing` | El aterrizaje |
| `double_jump.fbx` | `front flip` / `air spin` / `flip` | **Nueva.** El segundo salto, con giro |

Del doble salto: busca `flip` y quédate con una de **medio segundo o menos**,
que gire hacia adelante y termine de pie. Las de acrobacia larga no valen —
mientras dura la voltereta no se controla al personaje y se siente pesado.

## Tanda 2 — Trepar cornisas (prioritaria)

Lo más vistoso de todo lo que has pedido, y lo que más cambia cómo se lee el
mundo: un saliente deja de ser un muro y pasa a ser un camino.

| Nombre del archivo | Qué buscar | Para qué |
|---|---|---|
| `ledge_grab.fbx` | `hanging idle` o `braced hang` | Colgada del borde, quieta |
| `ledge_climb.fbx` | `climbing up` / `climb to top` / `braced hang to crouch` | Sube y se pone de pie |

Que las dos **empiecen y acaben en poses parecidas**: la de subir tiene que
arrancar donde termina la de colgarse, o se ve un salto entre las dos.

Ojo con la altura: Mixamo las hace para un borde a cierta altura del suelo. Al
bajarla, mira en el visor a qué altura queda la mano y dímelo — con eso ajusto
la detección para que la mano caiga en el borde de verdad y no en el aire.

## Tanda 3 — Conversación

Ya está bajada `talk_idle.fbx`, pero **no está conectada**. Con estas se puede
actuar de verdad:

| Nombre del archivo | Qué buscar | Cuándo se usa |
|---|---|---|
| `talk_idle.fbx` | ✅ ya la tienes | Bucle base mientras habla |
| `talk_gesture.fbx` | `talking` (la que mueve más las manos) | Frases con énfasis |
| `nod.fbx` | `head nod yes` | Decir que sí, estar de acuerdo |
| `shake_head.fbx` | `head shake no` | Negar, dudar |
| `point.fbx` | `pointing` | Señalar algo o a alguien |
| `surprised.fbx` | `surprised` / `reaction` | «¡Hay alguien ahí!» |
| `thinking.fbx` | `thinking` | Mercury explicando |

Estas son cortas y baratas. Cuantas más haya, menos se repite la conversación,
que es lo que más cansa de un juego con diálogo.

## Tanda 4 — Transformación

La actual no da la sensación de transformación. Busca en Mixamo:

- `magic spell` / `casting spell` — el gesto de invocar
- `praying` — más recogido, sirve para el arranque
- `standing up to action` — el momento de decisión

| Nombre del archivo | Qué buscar | Para qué |
|---|---|---|
| `transform_start.fbx` | `praying` o `standing idle to action` | Alza el broche |
| `transform_pose.fbx` | `magic spell` o pose de victoria vistosa | La pose final. Ya tienes una: cámbiala si encuentras mejor |

Bájate **dos o tres candidatas** de la pose final y las vemos en el juego. Esta
es de las que hay que ver en marcha para decidir; en el visor de Mixamo todas
parecen buenas.

---

## Orden recomendado

1. **Tanda 1** (salto) — desbloquea el doble salto, que es puro gusto de jugar
2. **Tanda 2** (trepar) — la que más cambia el diseño de nivel
3. **Tanda 3** (conversación) — arregla lo de Mercury saltando de golpe a alerta
4. **Tanda 4** (transformación) — la más de gusto personal, mándame varias

Con la tanda 1 y 2 dentro ya puedo montar la Fase E entera. Las otras dos van
con la Fase F.

---

## ✅ Integradas (2026-08-10)

Las 15 están dentro, con los nombres de las tablas de arriba. La librería
compartida tiene **25 animaciones** y se reconstruye con:

```bash
godot --headless --path . --script tools/construir_libreria_animaciones.gd
```

Mapeo de lo que bajó de Mixamo a los nombres del proyecto:

| Archivo de Mixamo | Nombre en el proyecto |
|---|---|
| `Jump` | `jump_start` (sustituye al anterior, guardado como `jump_start_viejo`) |
| `Falling To Landing` | `land` |
| `Front Flip` / `Front Flip-2` | `double_jump` / `double_jump_alt` |
| `Hanging Idle` | `ledge_grab` |
| `Climbing` | `ledge_climb` |
| `Talking` | `talk_gesture` |
| `Head Nod Yes` | `nod` |
| `Shaking Head No` | `shake_head` |
| `Pointing` | `point` |
| `Reacting` | `surprised` |
| `Thinking` | `thinking` |
| `Praying` | `transform_start` |
| `Standing Idle To Action Idle` | `transform_start_alt` |
| `Standing 2H Cast Spell 01` | `transform_pose_alt` |

Las `_alt` son candidatas para comparar en marcha, no entran en la librería.
Cuando se decida cuál se queda, se renombra y se reconstruye.

### Pendiente: el APK engordó

De 85 MB a 132. Se probó a excluir los `.fbx` con `exclude_filter` y solo bajan
14 MB, así que el peso está en otro sitio — los `.scn` importados de animación
son pequeños (876 KB el mayor). Hay que mirarlo con calma: no bloquea nada
porque se instala por cable, pero 130 MB para un parque es mucho.

## Cuando las tengas

Déjalas en `assets/animations/serena/` y dímelo. Yo me encargo de:

- Reconstruir la librería compartida
- Añadir los estados nuevos a la máquina de animación
- Conectar las mecánicas (doble salto, agarre de cornisa)
- Ajustar los tiempos de mezcla

No hace falta que toques nada de Godot.
