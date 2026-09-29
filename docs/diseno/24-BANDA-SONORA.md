> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/24-BANDA-SONORA.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# La banda sonora

## 1. Cómo se elige qué suena

No lo decide cada sitio por su cuenta. Hay un director —[`musica.gd`](../src/autoload/musica.gd)—
con una **pila de capas**: cada sitio *pide* su música y la *suelta* al terminar,
suena la de más prioridad que esté pedida, y al soltarla se vuelve sola a la de
debajo.

```
NADA       silencio a propósito, manda sobre todo
JEFE       el guardián
COMBATE    una arena en marcha
MOMENTO    una conversación, un rato especial
FONDO      el sitio donde se está: menú, parque, claro
```

Poner la música desde cada sitio parece más simple y se rompe enseguida: al
salir de una arena hay que acordarse de volver a la anterior, y si dos cosas
piden a la vez gana la última que hable, no la que importa. Con la pila, **nadie
tiene que acordarse de nada**.

### Las tres reglas

1. **Una canción por sitio, y se queda.** Cambiar de tema cada dos minutos
   marea. El cruce dura segundo y medio: lo justo para que no se note el corte
   sin que se solapen dos melodías.
2. **Al hablar baja, no se corta.** Cortarla deja un silencio que suena a error;
   bajarla a un tercio deja oír la voz sin que se note el cambio.
3. **El silencio también se usa.** Antes de que el guardián despierte no suena
   nada. Dos segundos de nada hacen más por ese momento que cualquier redoble —
   y su tema entra cuando ya está de pie, que es cuando significa algo.

## 2. Lo que hay puesto

Todo de [Incompetech](https://incompetech.com) (Kevin MacLeod), **CC-BY**: libre
incluso comercialmente a cambio de citar al autor. Está en
[`CREDITOS.md`](CREDITOS.md) y esa atribución **es la condición de uso**, no una
cortesía.

| Dónde suena | Pieza | Por qué esa |
|---|---|---|
| Menú y pantallas | *Fairytale Waltz* | Un vals de cuento, que es el registro que pide el juego. La primera elección —*Hidden Wonders*— sonaba oriental, y eso no lo detecta ninguna medida de tempo |
| El parque | *Enchanted Valley* | La más tranquila de todas — 27 acentos por minuto. Tiene que aguantar veinte minutos sin cansar |
| Arenas de combate | *Mischief Maker* | Traviesa y con ritmo (136/min). Son peluches, no monstruos |
| El guardián | *Heart of the Beast* | Lenta y pesada, como él |
| El claro del portal | *Magic Forest* | Quieta y abierta. Aquí no se pelea, se llega |
| Encuentros y momentos | *Dewdrop Fantasy* | Cálida y sin melodía marcada, para que no compita con la voz |

Recortadas a dos minutos, normalizadas al mismo nivel, con entrada y salida
suaves para que el bucle no chasquee. En `.mp3` a 96 kbps estéreo: son fondo, no
detalle, y **8,4 MB en total** frente a los 60 que ocuparían sin comprimir.

### Cómo se eligieron sin poder escucharlas

Midiendo **nivel medio y acentos por minuto** de cada candidata. Separa bien lo
tranquilo de lo movido, que es la decisión que más importa.

Lo que **no** separa es el carácter: la primera pieza de menú tenía el tempo
correcto y sonaba oriental, y eso no aparece en ninguna medida de energía. Para
eso lo único que sirve es el nombre y la descripción —«vals de cuento» dice más
del timbre que cualquier número— y, al final, escucharla.

Hay una alternativa guardada en `menu_alt.mp3` por si esta tampoco convence.

## 3. Las generadas con IA

Tres piezas que ninguna biblioteca iba a tener porque son de este juego. Las
generaste y las grabaste en vídeo; el audio se extrae del `.mov` con `ffmpeg`,
se recorta el silencio de cabecera y se normaliza al nivel de las demás.

| Pieza | Estado |
|---|---|
| **Créditos** | ✅ Puesta. Hay dos versiones; suena la v2 y la v1 queda guardada al lado — cambiar de una a otra es una línea en `musica.gd` |
| **Transformación** | ✅ Puesta. Doce segundos que suben, brillan y cierran. Antes iba prestada del ataque especial, y la transformación sonaba igual que un golpe cualquiera |
| **Victoria** | ⚠️ **El archivo está mudo.** Tiene pista de audio pero en silencio absoluto (−91 dB, el suelo digital): la grabación no capturó el sonido. Hay que volver a grabarla |

### Los prompts, por si hay que rehacer alguna

### El tema del título — todavía sin generar

Lo que suena al abrir. Debería ser **la melodía del juego** — la que se
reconozca, la que aparezca variada en el parque y en los créditos. Ahora mismo
el menú usa una pieza de biblioteca (*Hidden Wonders*), que cumple pero no es
suya.

```
A gentle, dreamy main theme for a children's fantasy game. Music box and soft
strings carry a simple, memorable melody in a major key, joined by light harp
and a warm pad. Nostalgic and hopeful, never grand or heroic. Slow tempo around
72 BPM, 4/4.

Structure: 8 bars of solo music box intro, then the full melody twice with
strings, ending on an unresolved chord so it loops back seamlessly.
Duration 90 seconds. No drums, no vocals, no sudden dynamic changes.
```

### La transformación

Seis u ocho segundos. **No es música de fondo: es un momento.** Sube, brilla y
termina — y tiene que encajar con la animación en tres tiempos que ya existe.

```
A short magical transformation cue for a children's game. Begins with a rising
harp glissando and shimmering bells, builds over 4 seconds with swelling strings
and a choir "ah", peaks on a bright major chord with a bell hit, then decays
into sparkles. Triumphant and sweet, never dark or intense.
Duration exactly 8 seconds. No drums, no vocals with words.
```

### La victoria del acto

Cuando cae el guardián y se abre la puerta. Doce segundos, y **es el único sitio
del juego donde se puede ser grandilocuente**.

```
A short victory fanfare for a children's fantasy game. Warm brass and strings
state a rising four-note figure, answered by harp and bells, resolving on a
sustained major chord with a soft cymbal swell. Celebratory and gentle — the
feeling of a door opening, not of a battle won.
Duration 12 seconds. No drums beyond a single timpani roll.
```

### Cómo pedirlas

Las tres siguen el mismo patrón, y la mitad de cada prompt son **requisitos
técnicos, no gustos**:

- **Duración exacta.** «Corto» no significa nada; 8 segundos sí.
- **Qué instrumentos NO.** Es más eficaz que decir cuáles sí — «sin batería»
  quita de golpe la mitad de lo que sale por defecto.
- **Cómo termina.** Un bucle necesita acabar sin resolver; un remate necesita
  resolver. Sin decirlo, salen todos igual.
- **El carácter, con dos adjetivos y su contrario.** «Nostálgica y esperanzada,
  nunca solemne» acota mucho mejor que tres adjetivos positivos seguidos.

Herramientas: **Suno** o **Udio** para piezas con estructura, **Stable Audio**
para los remates cortos, que es donde mejor se le da controlar la duración.

Al traerlas: las dejas en una carpeta y yo las recorto, normalizo al mismo nivel
que las demás y les pongo el bucle. Lo importante es que **el nivel case con las
que ya están**, o una pieza nueva sonará más alta que todo lo demás.

## 4. Lo que queda

- **Volver a grabar la victoria**, que salió muda.
- **El tema del título**, sin generar.
- Que la música del parque tenga **variante de noche o de zona** — hoy es una
  sola para todo el mapa.
- Un remate corto al recoger la última Estrella y al encender el último farol.
- Bajar la música también durante el panel de objetivos, no solo en los diálogos.
