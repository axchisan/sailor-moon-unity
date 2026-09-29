> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/16-GUION.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Guion — El Espejo de los Sueños

> La historia completa, los once personajes repartidos y el texto literal de las
> escenas. Existe para que cada nivel que se construya sepa **qué está
> contando**, y para no llegar al final con personajes sin sitio.
>
> Estructura de producción: [`15-PLAN-MAESTRO.md`](15-PLAN-MAESTRO.md).
> Reparto y estado de los modelos: [`12-REPARTO.md`](12-REPARTO.md).

## Para quién es

Una niña de ocho años. Eso manda sobre todo lo demás:

- **El conflicto no da miedo.** Nadie muere, nadie está en peligro de verdad. Lo
  que se pierde son los sueños, y se recuperan
- **Se entiende sin leer.** Todo lo importante se dice en voz alta o se ve
- **La villana da pena, no asco.** Al final se descubre por qué hace lo que hace,
  y se la ayuda en vez de destruirla
- **Las frases son cortas.** Nada de párrafos

---

## Premisa

Había un espejo que guardaba los sueños de toda la ciudad. Una noche se rompió,
y los sueños salieron volando convertidos en **Estrellas de Sueño**.

La **Dama del Espejo Roto** los va recogiendo, uno a uno, y se los queda. No por
maldad: se quedó atrapada dentro del espejo hace muchísimo tiempo y los sueños
de los demás son lo único que la acompaña. Cuanto más recoge, más gente se queda
apagada y gris — y ella más sola, porque nadie sale a la calle a soñar nada.

Serena tiene que recoger las Estrellas antes que ella, despertar a sus amigas y,
al final, **darle a la Dama un sueño propio** para que pueda salir.

### Por qué funciona

El giro final —no se la derrota, se la ayuda— es lo que separa esto de «matar
muñequitos». Y la mecánica lo sostiene: las Estrellas de Sueño que se recogen
por el mapa **son literalmente lo que la Dama quiere**. Recogerlas no es puntuar:
es llegar antes que ella.

---

## Los once, y dónde entra cada uno

| Personaje | Nivel | Papel |
|---|---|---|
| **Serena** / Sailor Moon | 1 | Jugable desde el principio |
| **Luna** | 1 | Acompaña siempre. La que explica |
| **Amy** / Sailor Mercury | 1 | Primera amiga. Encuentra dónde están las Estrellas |
| **Rei** / Sailor Mars | 2 | Fuego. Ve cosas que los demás no ven |
| **Lita** / Sailor Jupiter | 3 | Rayo. La fuerte |
| **Mina** / Sailor Venus | 4 | Cadena, alcance largo |
| **Artemis** | 4 | Llega con Mina |
| **Darién** / Tuxedo Mask | 5 | Aparece antes, se une aquí. Apoyo |
| **Neptuno y Uranus** | 6 | Van juntas. Ya sabían del espejo y no avisaron |
| **Pluto** | 7 | Guarda la puerta. Sabe quién es la Dama |
| **Saturno** | 7 | Puede romper el espejo del todo. Es la tentación |
| **Chibi Moon** | Post-créditos | El gancho |

### Lo mínimo y lo deseable

Ocho niveles son muchos. El reparto está pensado para poder **parar en el 4** y
que la historia cierre igual:

- **Núcleo (niveles 1-4 + final):** Serena, Luna, Mercury, Mars, Jupiter, Venus,
  Artemis y la Dama. Historia completa, principio y final
- **Ampliación (5-7):** Tuxedo, las Outers. Añaden, no arreglan
- **Gancho:** Chibi Moon, con el nombre de tu prima si quieres

Si en algún momento hay que recortar, se recorta por ahí y **no se nota**.

---

## Cinemática de apertura — ✅ montada

> Funciona en el juego y se ve al empezar una partida nueva. El texto está tal
> cual aquí abajo, en [`apertura.gd`](../src/ui/apertura.gd); el montaje —los
> tiempos, el movimiento de cámara, el fundido— en
> [`cinematica.gd`](../src/ui/cinematica.gd).
>
> **Las ilustraciones están dibujadas por código de momento**
> ([`dibujo_vineta.gd`](../src/ui/dibujo_vineta.gd)). No pretenden ser arte
> final: pretenden que la cinemática exista hoy y se puedan probar los tiempos.
> Los prompts para sustituirlas están al final de esta sección.

### Cómo está montado

- **Viñetas y no vídeo.** Un vídeo de minuto y medio son treinta megas y
  cambiar una frase obliga a renderizarlo entero. Así, cada viñeta es su imagen
  y su texto.
- **El movimiento es lo que la hace cinemática.** Una lámina con texto debajo es
  un libro; la misma lámina acercándose muy despacio mientras se lee es una
  escena. Cada viñeta define de dónde a dónde va la cámara.
- **El texto se escribe letra a letra**, con pausas más largas en las comas y
  los puntos. No es adorno: marca el tiempo. Una frase que aparece entera se lee
  en un segundo y deja seis de espera incómoda.
- **Se salta cuando se quiera**, y solo se ve **una vez por partida**. A la
  tercera muerte, minuto y medio de cuento es un peaje.

## Cinemática de apertura

Cinco viñetas fijas con movimiento lento de cámara. Voz de narrador. Minuto y
medio.

El texto va con las etiquetas de fish.audio, listo para generar. Voz de
narradora: cálida, de cuento antes de dormir. **No épica.**

### Viñeta 1 — El espejo entero

> *Ilustración: un espejo antiguo, ovalado, de marco dorado, flotando en un
> cielo nocturno lleno de estrellas. Dentro se ven reflejos de gente durmiendo.*

`[soft]` Hace mucho, mucho tiempo, sobre la ciudad de Tokio flotaba un espejo.

`[soft]` Nadie lo veía. Pero todas las noches, mientras la gente dormía, el
espejo guardaba sus sueños.

### Viñeta 2 — La grieta

> *El mismo espejo, ahora con una grieta que lo cruza. La luz se escapa por
> ella.*

`[sad]` Una noche, nadie sabe por qué, el espejo se rompió.

`[soft]` Y todos los sueños salieron volando.

### Viñeta 3 — La lluvia de estrellas

> *Vista de la ciudad de noche desde arriba. Cientos de estrellitas doradas
> cayendo sobre los tejados, los parques, las calles.*

`[soft]` Se convirtieron en Estrellas de Sueño, y cayeron por toda la ciudad.

`[soft]` En los parques. En los templos. En los tejados.

### Viñeta 4 — La Dama

> *Silueta de mujer elegante recortada contra un espejo roto, de espaldas,
> recogiendo una estrellita con la mano. Su reflejo se ve triste. Colores fríos:
> morados y azules.*

`[sad]` Pero alguien más las estaba buscando.

`[whisper]` La Dama del Espejo Roto vivía dentro del espejo. Sola. Desde hacía
tanto tiempo que ya no se acordaba de cuánto.

`[sad]` Y los sueños de los demás eran lo único que la acompañaba.

### Viñeta 5 — Serena y Luna

> *Serena dormida en su cama, abrazada a la almohada, con la boca abierta. Luna
> en la ventana, recortada contra la luna llena, mirándola.*

`[soft]` Sin sus sueños, la gente se iba quedando apagada. Y gris.

`[excited]` Por eso, esa mañana, una gata negra fue a buscar a una niña que
soñaba más que nadie.

`[excited]` Aunque casi siempre soñara despierta. Y en clase.

> *Corte a negro. Título: EL ESPEJO DE LOS SUEÑOS.*

### Nota de montaje

La última línea es el chiste que baja el tono justo antes de empezar a jugar.
Después de cuatro viñetas melancólicas hace falta, y además presenta a Serena
como es: buena persona y un desastre.

### Los prompts de las cinco ilustraciones

Para sustituir los dibujos por arte de verdad. Igual que con el guardián, la
mitad de cada prompt son **requisitos técnicos**: formato apaisado, sin texto y
con aire alrededor, porque la cámara se mueve por dentro de la imagen y si el
motivo llega al borde se sale del encuadre al acercarse.

Comunes a las cinco:

```
Soft anime storybook illustration, gentle pastel palette of deep purple, navy
and warm gold. Painterly but clean, no outlines. Night scene. Wide 16:9
landscape, generous empty margin around the subject, nothing touching the edges.
No text, no watermark, no borders, no characters looking at the camera.
```

| Viñeta | Prompt propio |
|---|---|
| 1 · El espejo entero | `An ornate oval mirror with a golden frame floating in a starry night sky above a sleeping city. Inside the glass, soft blurred reflections of people sleeping peacefully. Serene and warm.` |
| 2 · La grieta | `The same ornate oval mirror, now with a single jagged crack across it. Warm golden light escaping through the crack into the night sky. Sad but beautiful, not violent.` |
| 3 · La lluvia | `Aerial night view of a quiet Japanese city, rooftops and parks below. Hundreds of tiny golden star-lights drifting down over everything like slow snow.` |
| 4 · La Dama | `Silhouette of an elegant woman seen from behind, standing before a large shattered mirror, reaching up to catch a single golden star. Cool purples and blues. Lonely, never menacing.` |
| 5 · Serena y Luna | `A girl asleep in bed hugging her pillow, mouth open, blonde hair everywhere. A black cat sits on the windowsill silhouetted against a huge full moon, watching her. Warm and funny.` |

Cuando estén, se dejan en `assets/imagenes/apertura/` y en `apertura.gd` se
cambia el nombre del dibujo por la ruta del `.png`. El movimiento de cámara
sigue funcionando igual.

---

## Nivel 1 · Parque Juuban

**Qué enseña:** moverse, saltar, atacar, recoger Estrellas.
**Quién aparece:** Luna, Amy / Sailor Mercury.
**Estado:** ✅ construido.

### Apertura

Serena llega tarde al parque persiguiendo a Luna, que lleva toda la mañana
intentando hablar con ella y ella creyendo que es una gata normal.

**LUNA** `[excited]` — ¡Serena, espera! ¡Que te estoy hablando!
**SERENA** `[surprised]` — …¿Los gatos hablan? ¿Desde cuándo hablan los gatos?
**LUNA** `[soft]` — Desde siempre. Lo que pasa es que no escuchabas.

### Media parte — la primera Estrella

**LUNA** `[soft]` — ¿Ves eso que brilla? Es el sueño de alguien.
**SERENA** — ¿De quién?
**LUNA** `[sad]` — De cualquiera. De alguien que esta mañana se ha levantado
sin ganas de nada y no sabe por qué.

### El encuentro con Amy ✅ implementado

Amy está escondida junto al camino, midiendo algo con un aparatito.

Lo importante de esta escena: **Amy no necesita que la rescaten**. Ya sabía que
pasaba algo raro y ya estaba investigándolo. Se une porque quiere.

### Cierre — el guardián

Tras los tres faroles se abre el camino al guardián. Al ganar:

**LUNA** `[excited]` — ¡Lo habéis conseguido!
**AMY** `[soft]` — Mi ordenador dice que hay más Estrellas por toda la ciudad.
Muchas más.
**SERENA** `[determined]` — Entonces hay que ir a por ellas antes que ella.

---

## Nivel 2 · Templo Hikawa — Rei

**Qué enseña:** enemigos a distancia, cubrirse.
**Tono:** atardecer, escaleras de piedra, hojas rojas, cuervos.

Rei ya sabe que algo va mal: lo ha visto en el fuego. Está de mal humor porque
lleva días avisando y nadie le hace caso.

**REI** `[angry]` — Llevo tres días viéndolo en el fuego. Tres.
**SERENA** — ¿Y por qué no dijiste nada?
**REI** `[annoyed]` — ¡Lo dije! Nadie me escuchó.
**LUNA** `[embarrassed]` — …Eso es verdad.

**Gancho del nivel:** en el fuego, Rei ha visto la cara de la Dama. Y le sonaba.

---

## Nivel 3 · Escuela Juuban — Lita

**Qué enseña:** enemigos con escudo — hay que rodearlos.
**Tono:** pasillos vacíos, aulas a media luz, patio.

Lita es nueva, tiene fama de bruta y está sola. La única con su sueño **intacto**
en todo el edificio, y no sabe por qué.

**LITA** `[sad]` — Todos me miran raro. Igual es que soy rara.
**SERENA** `[soft]` — Yo también soy rara. Ven.

**Por qué su sueño resiste:** el suyo es pequeño y muy concreto —abrir una
pastelería— y los sueños concretos se agarran más fuerte. Lo dice Amy, y es la
pista para el final.

---

## Nivel 4 · Arcade Crown — Mina y Artemis

**Qué enseña:** enemigos rápidos en grupo, alcance largo.
**Tono:** neón, máquinas, música de recreativa.

Mina lleva semanas peleando sola con Artemis. Es la única que ya sabía ser
Sailor, y aun así no ha podido con esto ella sola.

**MINA** `[excited]` — ¡Por fin! ¡Sabía que apareceríais!
**ARTEMIS** `[tired]` — Lleva semanas diciendo eso. Todos los días.

**Cierre del acto:** con las cinco juntas, Luna explica lo del Cristal de Plata,
y Amy dice lo que nadie quiere oír: la Dama tiene ya **más Estrellas que ellas**.

---

## Niveles 5-7 · Ampliación

- **5 · Teatro** — Tuxedo Mask. Llevaba todo el juego apareciendo y
  desapareciendo sin explicarse. Aquí se explica: buscaba lo mismo por su cuenta
- **6 · Puerto** — Neptuno y Uranus. Sabían lo del espejo desde el principio y
  decidieron no avisar. Primera vez que las protagonistas discuten entre ellas
- **7 · Puerta del Tiempo** — Pluto y Saturno. Pluto sabe **quién era la Dama
  antes**. Saturno puede romper el espejo del todo y acabar con esto de una vez,
  y hay que decidir no hacerlo

---

## Final · La Torre del Espejo Roto

### Las tres fases

Tres, cada una vulnerable al poder de una Sailor distinta, para obligar a usar
al equipo entero. Pero **no son tres peleas**: son tres momentos.

1. **La Dama ataca.** Con los sueños robados, que aparecen como monstruos
2. **La Dama se defiende.** Se encierra en el espejo. Hay que romper los
   fragmentos, y en cada uno se ve un trozo de su pasado
3. **La Dama se queda quieta.** Ya no ataca. Y aquí no hay que pegarle

### El final

En la tercera fase el botón de atacar **no le hace nada**. Lo que funciona es
darle una Estrella de Sueño: la que se recogió al principio del juego, en el
parque, la primera de todas.

**SERENA** `[soft]` — Toma. Esta es mía.
**LA DAMA** `[confused]` — …¿Por qué?
**SERENA** `[soft]` — Porque los sueños no se guardan. Se cuentan.

El espejo se recompone. Los sueños vuelven a su gente. Y la Dama sale, por
primera vez en muchísimo tiempo.

### Cierre

Amanece en el parque Juuban, donde empezó todo. La ciudad despertando. Todas
juntas. Y una mujer más, que ya no es una silueta.

### Post-créditos

Una niña con coletas rosas cae del cielo encima de Serena.

**LA NIÑA** — ¿Tú eres Sailor Moon? Pues no eres como te imaginaba.

*(Aquí va Chibi Moon, o el nombre de tu prima. Es el mejor regalo posible dentro
del regalo: que se vea a sí misma en el juego.)*

---

## Qué hace falta producir

### Arte de la cinemática

Cinco ilustraciones fijas. Generadas aparte, en el estilo de la referencia:
colores pastel, brillo, línea suave.

Cada viñeta se anima con un movimiento lento de cámara —acercarse, desplazarse—
que es lo que convierte cinco dibujos quietos en una película.

### Voces

| Quién | Cuántas líneas | Prioridad |
|---|---|---|
| Narradora (apertura) | 11 | Alta — es lo primero que se oye |
| Luna | ~40 en todo el juego | Alta — es la que explica |
| Serena | ~60 | Alta |
| Las demás | ~20 cada una | Según el nivel |

Detalle de generación en [`14-VOCES.md`](14-VOCES.md).

### Música

- **Apertura:** caja de música, lenta, un poco triste
- **Parque:** alegre, ligera
- **Combate:** la misma con percusión encima. Que no cambie la melodía, solo la
  energía — así el cambio no corta
- **Final:** la caja de música de la apertura, ahora completa y en mayor

Que el tema del principio vuelva al final, transformado, es el truco más viejo
que existe y funciona siempre.
