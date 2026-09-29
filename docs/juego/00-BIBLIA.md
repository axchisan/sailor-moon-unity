# Biblia del juego — Sailor Moon: El Espejo de los Sueños

> **El documento que manda.** Si algo de otro documento lo contradice, gana
> este, y se corrige el otro. Escrito el 2026-09-29, cuando se decidió rehacer
> el diseño antes de construir: la versión de Godot eran niveles en fila que se
> acababan en 15 minutos y luego no quedaba nada que hacer.

## 1. En una frase

**Una aventura de ciudad abierta en la que Serena recorre Juuban devolviendo
los sueños robados a su gente, despierta a sus amigas como Sailor Scouts y
descubre quién es de verdad la Dama del Espejo Roto — y por qué no hay que
derrotarla, sino ayudarla.**

## 2. Para quién

Una niña de 8 años, hispanohablante, que conoce Sailor Moon y que va a jugar
en un teléfono Android en sesiones de 15–30 minutos durante semanas.

Eso decide:

- **Siempre sabe qué hacer.** Luna responde a «¿y ahora qué?» en cualquier
  momento, el mapa marca lo pendiente y el diario lo cuenta con dibujos. El
  fallo nº 1 de Godot («me lo pasé y no sabía qué hacer») no puede repetirse.
- **Siempre hay algo que hacer.** Si la historia se para, hay misiones, un
  minijuego, algo que coleccionar o un rincón por descubrir a menos de un
  minuto andando.
- **Nunca se pierde nada.** Sin *game over*, sin perder lo recogido, guardado
  automático constante.
- **Se entiende sin leer mucho.** Frases cortas, iconos, voz en lo importante.
- **Nada da miedo de verdad.** Lo que se pierde son sueños, y se recuperan.

## 3. Pilares

Cada decisión de diseño se comprueba contra estos cinco. Si no sirve a
ninguno, sobra.

1. **La ciudad está viva y cambia por lo que haces.** Cada sueño devuelto se
   ve: la señora vuelve a abrir su floristería, el niño vuelve a volar la
   cometa, la calle recupera el color. El mundo es el marcador.
2. **Ser Sailor Moon se siente increíble.** Transformarse, pegar y lanzar la
   Tiara tiene que dar gusto cada vez (el combate de Godot ya lo conseguía y se
   conserva).
3. **Las amigas importan.** Cada una tiene su historia, su barrio, su poder y
   su juego. Se echan de menos cuando no están y se nota cuando llegan.
4. **Explorar tiene premio.** Cada rincón esconde algo; lo que no se alcanza
   hoy se alcanza con el poder de la próxima amiga.
5. **Una historia con corazón.** La villana da pena, no miedo; el final se gana
   con un gesto, no con un golpe.

## 4. La estructura del juego

```
                    ┌──────────────────────────────┐
                    │   CALLE JUUBAN  (el centro)   │
                    │ casa de Serena · tiendas ·    │
                    │ Crown · estación de espejos   │
                    └──┬──────┬──────┬──────┬───────┘
                       │      │      │      │
                  PARQUE  TEMPLO  ESCUELA  CENTRO      → luego: BAHÍA, TORRE
                  (Amy)   (Rei)   (Lita)  (Mina)
```

- **Un centro** (la calle de Serena) al que siempre se vuelve: casa,
  habitación, tiendas, el Crown y la gente del barrio.
- **Barrios abiertos** que se desbloquean con la historia. Cada uno es una zona
  de juego libre con su capítulo principal, sus misiones, sus secretos, su
  amiga y su minijuego. Se viaja andando o por los **Espejitos** (puntos de
  viaje rápido que se reparan).
- **Orden en parte libre.** Tras el primer capítulo, el Templo y la Escuela se
  pueden hacer en el orden que se quiera; el Centro se abre con los dos.
- **Volver con poderes nuevos.** Cada amiga trae un poder de exploración
  (hielo, fuego, fuerza, cadena) que abre secretos en los barrios ya vistos.

Detalle en [`02-MUNDO.md`](02-MUNDO.md) y [`03-JUGABILIDAD.md`](03-JUGABILIDAD.md).

## 5. Duración objetivo

| Qué | Horas |
|---|---|
| Historia principal (prólogo, 4 capítulos de barrio, acto final) | 4–5 |
| Misiones de la gente (30 sueños perdidos) | 2–3 |
| Minijuegos, álbum, armario, secretos | 1–2 |
| Después del final (misiones de Rini, completar) | 1 |
| **Total al 100 %** | **8–10** |

Cuentas en [`08-CONTENIDO.md`](08-CONTENIDO.md).

## 6. Lo que se conserva de Godot y lo que cambia

| Se conserva | Cambia |
|---|---|
| La premisa: el Espejo, las Estrellas de Sueño, la Dama | La Dama tiene nombre, pasado y un porqué que se descubre poco a poco |
| El combate afinado (combo, especial, fichas de ataque, avisos rojos) | Se combate en arenas de la historia, en **Grietas** que aparecen por la ciudad y en retos |
| Los 11 personajes modelados | Todos tienen papel; las 5 principales son jugables y las demás se desbloquean |
| El Parque como primer barrio | Es uno de cinco, y abierto |
| Sin *game over*, avisos siempre | Más la ayuda de Luna, el mapa y el diario |
| Luna acompañando | Además es la pista, la guía y la narradora |

## 7. Índice

| Documento | Qué responde |
|---|---|
| [`01-LORE.md`](01-LORE.md) | Por qué pasa lo que pasa: el Espejo, la Dama, las Estrellas, los enemigos |
| [`02-MUNDO.md`](02-MUNDO.md) | Dónde: la ciudad, cada barrio, qué hay y cómo se desbloquea |
| [`03-JUGABILIDAD.md`](03-JUGABILIDAD.md) | Qué se hace: bucles, poderes, progresión, economía, ayudas |
| [`04-HISTORIA.md`](04-HISTORIA.md) | La historia por actos y capítulos, con sus escenas clave |
| [`05-PERSONAJES.md`](05-PERSONAJES.md) | Quién es quién, qué quiere y cómo cambia |
| [`06-MISIONES.md`](06-MISIONES.md) | Los 30 sueños perdidos de la gente de la ciudad |
| [`07-ACTIVIDADES.md`](07-ACTIVIDADES.md) | Minijuegos, álbum de cartas, armario, habitación |
| [`08-CONTENIDO.md`](08-CONTENIDO.md) | Cuánto hay de cada cosa, horas y qué hay que producir |
| [`09-ROADMAP.md`](09-ROADMAP.md) | En qué orden se construye todo |
| [`guiones/`](guiones/) | El texto literal, capítulo a capítulo |

## 8. Tono y estilo

- **Referencia:** la etapa luminosa del anime (Sailor Moon S y SuperS): pastel,
  brillo, humor, amistad. El conflicto es de sueños, no de destruir el mundo.
- **Nombres del doblaje latinoamericano:** Serena, Luna, Amy, Rei, Lita, Mina,
  Artemis, Darién, Molly, Andrew, Melvin, Sammy, Rini. Las Outers con sus
  nombres (Haruka, Michiru, Setsuna, Hotaru).
- **Humor de Serena:** torpe, dormilona, glotona, llorona y valiente cuando
  importa. Es la fuente de la mitad de las risas y de todo el corazón.
- **Frases de 12 palabras como máximo** en diálogo normal; las escenas grandes
  pueden tener alguna más larga, nunca un párrafo.

## 9. Nota legal

Sailor Moon es propiedad de Naoko Takeuchi / Kodansha / Toei Animation. Regalo
personal, no distribuido ni monetizado. La historia de este juego es original
y no forma parte de ninguna continuidad oficial.
