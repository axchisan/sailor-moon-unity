> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/20-JEFE-GUARDIAN.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# El Guardián del Portal

> **Hecho y en el juego.** Modelo, esqueleto, ocho animaciones y la pelea con
> sus tres fases. Lo que queda al final del documento son los remates.

Cierre del Acto 1. Aparece cuando la lista de objetivos está completa y se
planta delante de la ruina: la puerta no se abre porque sí, se abre porque
alguien la deja abrir.

## El concepto

**Un guardián de piedra del propio portal.** Está hecho de la misma roca que la
ruina, con las mismas vetas y los mismos cristales rosados incrustados. Lleva
ahí desde antes que el parque, dormido, y lo que lo despierta es que alguien
haya reunido todo lo que hacía falta.

Que sea de la piedra del portal no es decoración: es lo que hace que verlo se
entienda sin una línea de diálogo. Si fuera un monstruo cualquiera, sería un
enemigo más y grande; siendo la puerta la que se levanta, es el último examen.

**No da miedo, impone.** Es para una niña de ocho años: nada de colmillos,
garras ni caras de rabia. La fuerza tiene que venir del tamaño, de lo despacio
que se mueve y de lo que pesa cada golpe — no de la cara. Ojos grandes y
tranquilos, de luz rosa. Un gigante serio, no un monstruo.

Musgo y florecitas en los hombros: llevaba tanto tiempo quieto que el parque le
creció encima. Eso, además de ser bonito, cuenta los siglos sin decirlos.

## El prompt para generar la imagen

Va en inglés, que es donde estos modelos aciertan más. Todo lo de la segunda
mitad son **requisitos del paso imagen → 3D**, no gustos: una imagen bonita con
la pose girada o con sombras duras da un modelo inservible.

```
Full-body character concept of a gentle stone guardian, front view, standing
straight in a symmetrical A-pose with arms held away from the body and hands
open, legs slightly apart, facing the camera directly.

Design: a large ancient guardian carved from pale weathered granite, broad
square shoulders, long heavy arms, short thick legs, no neck. Chunky simplified
anime style, soft rounded shapes, clean readable silhouette. Glowing rose-pink
crystals embedded in its chest, shoulders and knuckles, the same crystal as an
old stone gate. Carved crescent-moon motif on its chest. Soft green moss and
tiny pink flowers growing on its shoulders and forearms. Large calm glowing pink
eyes, no mouth, no teeth, no claws — serene and protective, not scary.
Cel-shaded cartoon rendering with flat pastel colors, thin dark outlines, minimal
shading, in the style of a stylized low-poly mobile game.

Plain flat light-gray background. Even soft frontal lighting, no cast shadows,
no rim light, no dramatic lighting. Entire body visible from head to feet with
margin around it, nothing cropped. No ground, no floor, no pedestal, no scenery,
no particles, no glow effects, no motion blur, no text, no watermark, no
multiple views, single character centered in frame.
```

### Variante, si el primero sale demasiado humano

Cambiar el bloque de diseño por:

```
Design: a floating guardian made of separated stone slabs held together by pink
light — a broad chest slab, two large detached fists hovering where hands would
be, a smooth carved head with no face except two glowing pink eyes, and a
tapering base instead of legs.
```

Se mueve mejor con menos huesos y da un jefe que flota, que pelea distinto a
todo lo del nivel.

## Qué mirar antes de pasarla a 3D

- **La pose.** Brazos despegados del cuerpo y manos abiertas. Si los brazos
  tocan el torso, el reconstructor los funde en un bloque y luego no hay forma
  de separarlos para animar.
- **Que no esté cortada.** Ni un dedo fuera del encuadre.
- **Sin sombras duras ni brillos.** El reconstructor las interpreta como
  relieve y salen bultos donde no los hay.
- **Fondo liso.** Cualquier cosa detrás acaba pegada al modelo.
- **Una sola vista.** Las láminas de «model sheet» con tres poses dan tres
  medio-modelos superpuestos.

## Cómo se montó

El modelo llegó con **dos millones de triángulos**, 74 MB y sin esqueleto. Lo
que se hizo con él, por si hay que repetirlo con otro:

1. **Reducir a 25 000 triángulos** (Decimate, ratio 0,0125). Es UN jefe en
   pantalla, no una multitud, así que se le puede dar más que a un peluche —
   pero dos millones no los mueve ningún móvil. A 25k no se nota la diferencia:
   siguen ahí los cristales, el musgo y las florecitas.
2. **Texturas de 4K a 1024**, y fuera la de *metallic/roughness*: el juego pinta
   con MToon y no la mira. Eran ocho megas para un canal que nadie lee. Con eso
   el `.glb` pasó de 74 MB a **1,3 MB**.
3. **Esqueleto de 19 huesos**, puesto por código con las medidas sacadas de
   dónde está cada pieza — no a ojo.
4. **Pesos rígidos**: cada vértice a un solo hueso y con peso entero. Lo normal
   es repartirlos para que la piel se doble suave, pero esto es un golem de
   placas de piedra y la piedra no se estira. Repartir pesos aquí solo consigue
   que las placas se deformen como goma en los codos, que es lo que delata a un
   muñeco barato.
5. **Ocho animaciones** a mano en Blender.

El `.blend` está en `Models/guardian_portal.blend`. Si hay que retocar una
animación o añadir otra, se abre ese — rehacer el rig desde el `.glb` original
de dos millones de triángulos no es una opción.

## Las animaciones

| Nombre | Qué es |
|---|---|
| `reposo` | Casi nada, a propósito. Un golem que se mueve mucho parado deja de pesar |
| `andar` | El peso está en el balanceo lateral, no en la zancada |
| `despertar` | Cinco segundos desde el ovillo hasta ponerse de pie. Es su presentación |
| `golpe` | Anticipación larga y bajada seca |
| `golpe_suelo` | Los dos puños arriba y abajo — el ataque de área |
| `barrido` | El brazo a ras de suelo, cruzando. **Se salta** |
| `gancho` | Las dos manos de abajo arriba. Saltar aquí es lo peor que se puede hacer |
| `embestida` | Baja el hombro y va. Castiga quedarse lejos pegando |
| `rugido` | El aviso entre fases |
| `dano` | Corto y con poco recorrido: si se tambalea con cada golpe, deja de imponer |
| `derrota` | No explota. Se le apaga la luz y se sienta a quedarse quieto otra vez |

## Cómo pelea

No estrena mecánicas. Un jefe que pide algo que el nivel no ha enseñado no es
difícil, es injusto — así que es lo mismo de siempre con más peso: pega despacio
y avisando, y a cambio el golpe duele y empuja lejos.

**Gira lento** (`turn_speed` 2,6 contra los 8-9 de los demás). Ahí está toda la
pelea: rodearlo funciona, quedarse delante no. Es la lección del Cofrecito otra
vez, contra algo que no se puede matar a base de insistir.

**Tres fases**, a dos tercios y a un tercio de vida. Se para, ruge y acelera.
Sirve para partir una pelea larga en tres tramos —una barra que baja sin que
pase nada se hace eterna a los ocho años— y para avisar de que la cosa cambia
antes de que cambie, en vez de después.

### Cinco ataques, y cada uno pide otra cosa

Con uno solo la pelea se resolvía de una manera y ya: alejarse un paso,
acercarse a pegar, repetir. **Un jefe con un ataque no tiene ritmo, tiene un
bucle** — y en cuanto se le pilla, deja de haber pelea.

| Ataque | Cuándo lo usa | Qué hay que hacer |
|---|---|---|
| Puño | Cerca | Rodearlo |
| Barrido | Cerca | **Saltar** — apartarse no basta, es ancho |
| Gancho | Muy cerca | **Salir de delante** — saltar es meterse en él |
| Golpe al suelo | Media | **Alejarse**: es un círculo entero |
| Embestida | Lejos | Apartarse **cuando ya se ha lanzado** |

Se elige filtrando por distancia y descartando el último usado. Lo de descartar
el último es lo que más se nota: sin eso el azar repite, y dos barridos seguidos
se leen como que el juego está roto, no como mala suerte.

**El seguimiento es distinto en cada uno**, y ahí está la mitad del asunto. El
barrido apenas corrige la puntería, así que esquivarlo de lado funciona; la
embestida corrige mucho al principio y nada al final, así que hay que esperar a
que se comprometa. Si todos siguieran igual, la respuesta sería siempre la misma
y daría lo mismo cuál viniera.

Ninguno persigue hasta el instante del impacto. Un ataque que te sigue hasta el
final no se puede esquivar, solo aguantar.

### El aguante: sin esto no había pelea

Cada golpe recibido lo mandaba a `Stagger`, así que **pegando rápido se le
encadenaba indefinidamente** y no llegaba a atacar ni una vez. «Ni se pudo
defender» era literal, no una forma de hablar.

Ahora encaja tres seguidos sin inmutarse y acusa el cuarto —cinco en la última
fase—. Se sigue viendo que le entran, con su parpadeo y su chispa, pero no se le
puede dejar clavado. Y **en plena carga de un ataque no se aturde nunca**: lo
que ha empezado, lo termina. Ese es el trato — le puedes meter todo lo que
quieras, pero el golpe que ya viene te llega.

## Lo que queda

- **Barra de vida del jefe.** Sin ella la pelea no dice cuánto falta. Va con el
  panel de objetivos, cuando haya interfaz.
- **Música propia.** Es el único sitio del acto que la pide.
- **Sonido de verdad.** Lo que suena ahora es provisional y de juego retro; hace
  falta grabación realista. La lista de qué descargar está en
  [`21-SONIDO-Y-MUSICA.md`](21-SONIDO-Y-MUSICA.md).
