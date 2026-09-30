# Roadmap — en qué orden se construye

> Sustituye a `docs/PLAN.md` (que queda como historia). Construido sobre el
> diseño de esta carpeta: si el diseño cambia, este documento se revisa.

## Principios

1. **Primero los sistemas, luego el contenido, y el contenido en rebanadas
   completas.** Nunca cinco barrios a medias: un barrio entero, jugable y
   bonito, y después el siguiente.
2. **Cada fase acaba con un APK en el teléfono** y una prueba jugando.
3. **Todo el contenido son datos** (*ScriptableObjects* que se editan en el
   Inspector): misiones, diálogos, Estrellas, cartas, ropa, ataques, oleadas.
   Añadir una misión nueva no debe pedir programar.
4. **La producción de arte va en paralelo** desde la fase 3 (§Carriles).
5. **Rendimiento medido en cada fase**, en el HONOR.

## Hitos para la prima

| Hito | Tras la fase | Qué juega | Horas |
|---|---|---|---|
| **H1 · Juuban** | 4 | La calle, su casa, el prólogo, el Parque entero con Amy, primeras misiones, su primer minijuego, álbum y habitación | 1,5–2 |
| **H2 · El equipo** | 7 | + Templo y Escuela, Rei y Lita | 4–5 |
| **H3 · La historia entera** | 9 | Hasta el final | 7–8 |
| **H4 · Completo** | 10 | Todo, con el después del final | 9–10 |

## Fases

| Fase | Qué | Sesiones | Estado |
|---|---|---|---|
| **0** | Cimientos, Serena moviéndose, medidor, medida en el HONOR | — | ✅ |
| **1** | Serena completa | 4–5 | 🟡 escala, terreno y combate hechos |
| **2** | Combate completo: Pesadillas, Grietas, Temor | 4–5 | ⬜ |
| **3** | Los sistemas del mundo | 6–7 | ⬜ |
| **4** | **Rebanada vertical: Calle Juuban + Parque** → H1 | 8–10 | ⬜ |
| **5** | Templo (cap. 2) | 6–8 | ⬜ |
| **6** | Escuela (cap. 3) | 6–8 | ⬜ |
| **7** | Distrito de las Luces (cap. 4) → H2 al cerrar 6, H2+ aquí | 7–9 | ⬜ |
| **8** | Acto III: Bahía, el pasado, el Espejo, el final → H3 | 7–9 | ⬜ |
| **9** | Después del final y pulido → H4 | 5–7 | ⬜ |

**Total orientativo: 55–70 sesiones.** Como referencia, el Nivel 1 de Godot
fueron unas 17. La rebanada vertical (fases 1–4) es un tercio del trabajo y
deja hechos casi todos los sistemas: los barrios siguientes van más rápido.

---

### Fase 1 — Serena completa

- Movimiento: doble salto, agarrar cornisas y trepar (Animation Rigging).
- Combate: `AttackData`, combo de 3, auto-orientación, cajas activadas por
  *Animation Events*, línea de vista, `CombatFeel` (parada de impacto,
  Cinemachine Impulse, partículas, destello, sonido).
- Especial con barra; vida de corazones; mareo y reaparición.
- **Transformación** con Timeline (larga la primera vez, corta después).
- **Tiara Lunar** como primer poder de campo (base del sistema de poderes).
- **Terreno toon con luz horneada** (arregla U5; el terreno era el 37 % del
  fotograma). Se hace aquí porque todo lo demás se construye encima.

**Hecha cuando:** en el teléfono, se pega a muñecos con gusto, se lanza la
Tiara a un interruptor y el sitio canónico baja de 14 ms de GPU.

### Fase 2 — Combate completo

- Plantilla de Pesadilla (`EnemyData`, máquina de estados, NavMesh).
- Peluchín, Cofrecito, Hadita (y sus versiones grande y dorada).
- Director de combate: 4 fichas, aviso rojo, compasión.
- **Arena** como prefab y **Grieta** como arena que aparece en el mundo.
- **Temor** (el guardián) con sus fases y el momento de Amy.
- **Cambio de Sailor** (con Mercury como segunda jugable de prueba).

**Hecha cuando:** una Grieta y la pelea con Temor se juegan enteras en el
teléfono a 60 fps.

### Fase 3 — Los sistemas del mundo

Todo lo que hace que un mundo abierto funcione, probado en una escena gris.

| Sistema | Qué |
|---|---|
| **Guardado** | 3 partidas, JSON con versión, automático |
| **Arranque y carga** | Boot, menú, cargar barrios por escenas con transición; precarga y shaders calentados (arranque < 5 s) |
| **Diálogos** | `DialogueData`: quién, retrato, expresión, voz, animación; frases alternativas según el grupo |
| **Misiones** | `QuestData`: pasos, condiciones, premio; el tablón de Molly |
| **Estrellas** | `DreamStarData`: dueño, dónde está, estado (perdida, recogida, devuelta) |
| **Gente** | NPC con rutina sencilla, globo «!», frases antes/después de su sueño |
| **Color de barrio** | Desaturación por zona que se recupera con el porcentaje |
| **Ayudas** | Botón de Luna, diario, mapa, flecha guía |
| **Economía** | Chispas, tiendas, inventario (comida, ropa, muebles) |
| **Álbum** | `CardData` y la pantalla del álbum |
| **Poderes de campo** | Base genérica: objetos que reaccionan a hielo, fuego, fuerza, cadena |
| **Espejitos** | Viaje rápido y recuerdos |
| **Audio** | AudioMixer, director de música por capas y por estado del barrio |

**Hecha cuando:** en una escena de pruebas se completa una misión de tres pasos
con diálogos, se devuelve una Estrella y el sitio cambia de color, se guarda,
se cierra el juego y al volver todo sigue igual.

### Fase 4 — Rebanada vertical: Calle Juuban y Parque

El primer trozo **completo** del juego. Todo a mano en el editor.

- **Calle Juuban**: calle, casa Tsukino (habitación, cocina, salón), Crown,
  boutique, plaza con el Espejito, tablón.
- **Parque Juuban** rehecho y abierto: las seis sub-zonas, faroles, estanque.
- **Prólogo y capítulo 1** completos con sus guiones
  ([`guiones/01-PROLOGO.md`](guiones/01-PROLOGO.md), [`guiones/02-CAP1-PARQUE.md`](guiones/02-CAP1-PARQUE.md)).
- **Mercury** jugable con Hielo y Visor.
- **Misiones** C1, C2, C5, C8, P1, P2, P4, P5 (las que no piden poderes de después).
- **Burbujas de Mercurio**.
- **Habitación** con 10 muebles, **armario** con 5 prendas, **álbum** con las
  cartas de este tramo.
- **Estrellas ocultas** del parque y la calle; 2 Espejitos pequeños.
- Música de la calle y el parque (gris y alegre); la apertura.

**Hecha cuando:** la prima juega H1 de principio a fin **sin que nadie le
diga qué hacer**, y quiere seguir.

### Fases 5, 6 y 7 — Templo, Escuela, Distrito

Cada una igual: el barrio a mano, su capítulo con su guion, su amiga jugable
con su poder y su estilo de combate, su Pesadilla nueva, su Reflejo, su
minijuego, sus misiones, sus Estrellas ocultas, sus secretos (incluidos los que
abren poderes en barrios anteriores), su música y su interludio en el Crown.
La fase 7 añade el **ataque combinado** y la recreativa de Sailor V.

### Fase 8 — Acto III

Bahía y las Outers, el tramo jugable del pasado (el palacio de la Luna), el
interior del Espejo, los Reflejos aceptados, la Dama en tres momentos, el
epílogo y los créditos con la gente ayudada.

### Fase 9 — Después del final y pulido

Rini y sus misiones, el Jardín del Espejo con Lira, las Outers y Chibi Moon
jugables, el álbum y el armario completos, día y noche (si cabe), voces,
accesibilidad, rendimiento final y la versión para instalarle a la niña.

---

## Carriles de producción (en paralelo desde la fase 3)

El código y el arte avanzan a la vez. Mientras se programan los sistemas se van
preparando los modelos del barrio siguiente.

| Carril | Qué | Herramienta | Se necesita para |
|---|---|---|---|
| **Gente** | ~30 civiles + figurantes | hi3d.ai → Blender → Mixamo → `importar` | Cada barrio |
| **Kits** | Un kit de props por barrio | Script de Blender por kit | Cada barrio |
| **Enemigos** | 4 Pesadillas + 3 Reflejos + Dama | hi3d.ai | Fases 2, 5–8 |
| **Música** | ~12 piezas | Generación como en Godot | Cada barrio |
| **Voces** | Escenas clave | fish.audio | Cada capítulo |
| **Guiones** | Texto de cada capítulo y misión | Esta carpeta | Antes de cada fase de barrio |

**Regla:** un barrio no empieza a construirse sin su guion escrito, su kit y
su gente listos (o al menos en gris).

## Guiones: qué hay escrito

| Guion | Estado |
|---|---|
| [`guiones/00-APERTURA.md`](guiones/00-APERTURA.md) | ✅ |
| [`guiones/01-PROLOGO.md`](guiones/01-PROLOGO.md) | ✅ |
| [`guiones/02-CAP1-PARQUE.md`](guiones/02-CAP1-PARQUE.md) | ✅ |
| Capítulos 2–6, interludios, misiones, Rini | ⬜ en las próximas sesiones, siempre antes de su fase |
