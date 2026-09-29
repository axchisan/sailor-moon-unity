# Jugabilidad — qué se hace y por qué engancha

## 1. Los tres bucles

Un juego con contenido se sostiene en tres ritmos a la vez. En Godot solo
existía el primero, y por eso se acababa en 15 minutos.

| Bucle | Dura | Qué se hace | Qué se gana |
|---|---|---|---|
| **Del minuto** | 30 s – 2 min | Explorar, pelear un grupo de Pesadillas, recoger, abrir un secreto | Chispas, una Estrella, una carta |
| **De la tarde** | 10–30 min (una sesión) | Una misión: un capítulo de la historia, el sueño de un vecino, un minijuego | Una Estrella devuelta, un barrio más colorido, ropa, un mueble, un recuerdo de Luna |
| **De las semanas** | Todo el juego | Colorear la ciudad entera, llenar el álbum, subir el Cristal, despertar a todas las amigas | Poderes nuevos, barrios nuevos, la verdad sobre la Dama |

**Regla de diseño:** al terminar cualquier cosa, **siempre hay otra a la vista**
(un icono en el mapa, alguien con un globo de «!», Luna que sugiere algo).

## 2. Una tarde de juego (ejemplo)

> Serena sale de casa. En el tablón hay un dibujo nuevo: al señor de los
> crepes le falta su sueño. Luna dice que brilla algo en el parque. De camino,
> una Grieta se abre en la plaza: tres Peluchines, un combo, el especial,
> Chispas. En el parque, la Estrella está en el islote del estanque: no se
> llega. Luna: «Con el hielo de Amy podrías cruzar». Serena cambia a Mercury,
> congela el agua, recoge la Estrella y encuentra además una carta de Amy
> escondida. Vuelve a la calle, se la da al señor de los crepes: su puesto se
> llena de color y regala un crepe de fresa. El Cristal sube de brillo:
> un corazón más. Serena se compra un lazo nuevo con las Chispas y se lo pone.

Veinticinco minutos, seis cosas distintas, y todo conectado.

## 3. Poderes de exploración

Cada Sailor trae un poder que sirve **fuera del combate** para moverse y
descubrir. Se cambia de Sailor en cualquier momento con el menú radial
(fuera de las peleas de jefe, donde está limitado para que cuente la historia).

| Sailor | Poder de campo | Abre | Se consigue |
|---|---|---|---|
| **Moon** | **Tiara Lunar** (lanzar y que vuelva) | Interruptores lejanos, cristales grises, recoger cosas lejanas | Prólogo |
| **Mercury** | **Hielo** (congelar agua) y **Visor** (ver lo oculto) | Cruzar estanques y fuentes; ver Estrellas y caminos invisibles | Capítulo 1 |
| **Mars** | **Fuego** | Quemar zarzas grises, encender faroles y braseros que abren puertas | Capítulo 2 |
| **Jupiter** | **Fuerza del Trueno** | Empujar y levantar bloques, romper rocas, dar corriente a máquinas | Capítulo 3 |
| **Venus** | **Cadena del Amor** | Engancharse a corazones colgados para cruzar huecos; atraer cosas | Capítulo 4 |

- **Cada barrio tiene secretos para cada poder.** Así cada amiga nueva abre
  contenido en todos los barrios anteriores: la ciudad crece hacia atrás.
- **Los secretos se ven antes de poder abrirse** (una zarza gris, un islote
  brillando): la jugadora sabe que ahí hay algo y a qué volver. Luna lo apunta
  en el diario.

## 4. Combate

Se conserva el diseño afinado en Godot ([`../diseno/05-COMBATE.md`](../diseno/05-COMBATE.md),
[`../diseno/00-GDD.md`](../diseno/00-GDD.md) §5): combo de 3 golpes con
ventana de 0,8 s, auto-orientación, especial con barra, máximo 4 enemigos
atacando, aviso rojo de 0,75 s, 1,5 s de invulnerabilidad, sin *game over*.

Lo que se añade:

| Novedad | Qué es |
|---|---|
| **Estilo por Sailor** | Mismo esqueleto y animaciones, distinto efecto: Moon equilibrada (el 3er golpe lanza la Tiara), Mercury ralentiza, Mars deja fuego, Jupiter rompe escudos, Venus alcanza lejos |
| **Cambio en combate** | Cambiar de Sailor en mitad de una pelea (con un tiempo de espera corto). Algunas Pesadillas piden una Sailor concreta |
| **Ataque combinado** | Con dos o más amigas despiertas y la barra llena: ataque de equipo con escena corta. El premio de juntar al grupo |
| **Dónde se combate** | Arenas de la historia; **Grietas** que se abren por la ciudad; **Retos** de cada barrio; jefes |

### Grietas (combate repetible)

Sitios donde tocó el Espejo. Cada cierto tiempo de juego (y siempre al entrar
de noche) se abre alguna, marcada en el mapa. Dentro: 2–3 oleadas cortas,
barrera mágica y premio de Chispas y a veces una carta. **Da algo que hacer
siempre** y es donde se practica con cada Sailor.

### Retos del barrio

Tres por barrio, en un poste con una estrella: «vence a 10 sin que te toquen»,
«gana con Mars», «termina en menos de 1 minuto». Dan cartas especiales y ropa.
Opcionales, para quien quiera más.

## 5. Progresión

### El Cristal de Plata (fuerza)

Sube con las **Estrellas devueltas** (a su dueño, no solo recogidas):

| Brillo | Estrellas | Premio |
|---|---|---|
| 1 | 0 | 5 corazones, combo básico |
| 2 | 5 | +1 corazón |
| 3 | 12 | Especial más grande |
| 4 | 20 | Golpe cargado (mantener el botón) |
| 5 | 30 | +1 corazón |
| 6 | 40 | La barra del especial se llena más rápido |
| 7 | 52 | Ataque combinado más fuerte |
| 8 | 65 | +1 corazón |
| 9 | 80 | Traje «Super Sailor» (aspecto) |
| 10 | 100 | Aura dorada (aspecto) y la carta secreta del álbum |

La historia principal da unas 35 Estrellas: **se puede terminar sin hacer
secundarias**, pero con ellas se llega mucho más fuerte y se ve todo.

### Amigas (poderes y variedad)

Una por capítulo: cada una trae su poder de campo, su estilo de combate, su
minijuego y su historia.

### La ciudad (el marcador)

Cada barrio tiene un **porcentaje de color** visible en el mapa: Estrellas
devueltas, Espejitos reparados y secretos. Llegar al 100 % en un barrio da una
fanfarria, un mueble especial y una carta dorada.

## 6. Economía: las Chispas

| Se ganan | Cuánto (orientativo) |
|---|---|
| Pesadilla vencida | 1–3 |
| Grieta cerrada | 20–40 |
| Misión de un vecino | 30–80 y a veces un objeto |
| Minijuego | 10–50 según la puntuación |
| Objetos rompibles (jarrones, cajas grises) | 1–5 |
| Secreto | 25–100 |

| Se gastan | Precio |
|---|---|
| Accesorio (lazo, diadema, gafas) | 50–150 |
| Conjunto de ropa | 200–400 |
| Mueble o peluche para la habitación | 80–300 |
| Sobre de 3 cartas (el Crown) | 60 |
| Comida que cura (crepes, batidos, pasteles de Lita) | 10–30 |

Nada imprescindible para la historia se compra: **las Chispas son para
disfrutar**, nunca un muro.

## 7. Que siempre sepa qué hacer

La ayuda es parte del diseño, no un parche. Cinco capas, de menos a más:

1. **El mundo lo enseña**: lo importante brilla, las personas con misión llevan
   un globo «!», los secretos se ven aunque no se puedan abrir.
2. **Luna**: un botón con su cara. Al tocarlo dice en una frase, con voz, qué es
   lo siguiente de la historia **y** una sugerencia cercana («la señora de las
   flores está triste, ¿vamos?»).
3. **El diario** (menú): misiones con dibujo, cuántas Estrellas faltan por
   barrio, secretos vistos y qué poder piden.
4. **El mapa**: iconos de misión, Grietas abiertas, Espejitos, y el porcentaje de
   cada barrio.
5. **La flecha guía**: se elige cualquier cosa del diario y una flecha la señala
   (se conserva de Godot).

## 8. Dificultad

- **Por defecto, generosa** (la de Godot, ya probada con la jugadora).
- **Modo asistido** en ajustes, sin anunciarlo: más corazones, enemigos más
  lentos, avisos más largos.
- **Ajuste automático suave**: si se marea dos veces seguidas en la misma arena,
  la tercera los enemigos pegan un poco menos. No se dice; se nota.

## 9. Guardado

- **Automático** al entrar en un barrio, al terminar cualquier misión y al
  dormir.
- **Tres partidas** (se conserva de Godot).
- Se guarda: historia, Estrellas (recogidas y devueltas), barrios, secretos,
  Chispas, ropa, muebles, cartas, puntuaciones de minijuegos, Sailor desbloqueadas.

## 10. Pantallas

| Pantalla | Qué tiene |
|---|---|
| Título | Partidas, ajustes, créditos |
| Juego (HUD) | Corazones, barra de especial, Chispas, botón de Luna, menú radial de Sailor, minimapa pequeño |
| Menú de pausa | Mapa · Diario · Álbum · Armario · Ajustes |
| Habitación | Decorar, armario, álbum, dormir, guardar |
| Tiendas | Escaparate con precio en Chispas |
| Minijuegos | Cada uno con su pantalla |
