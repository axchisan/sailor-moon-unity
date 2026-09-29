# Plan de desarrollo

> **Decidido el 2026-09-29:** el juego se construye en Unity. Este plan lleva
> desde los cimientos de hoy hasta el juego completo: primero **lo que había en
> Godot, hecho mejor**, y después lo que en Godot nunca llegó a existir.
>
> Se actualiza al cerrar cada fase. El detalle del día a día va en
> [`ESTADO.md`](ESTADO.md).

## Cómo se trabaja

1. **Una fase detrás de otra, y cada fase se cierra jugable.** Nada de dejar
   cinco cosas a medias: cada fase acaba con un APK instalado en el teléfono y
   una prueba jugando.
2. **Primero la base que usa todo lo demás.** El combate antes que los enemigos,
   los enemigos antes que el nivel, el marco del juego (guardado, menús) antes
   de que haya contenido que guardar.
3. **Todo lo que se pueda ajustar, en datos editables.** Ataques, enemigos,
   oleadas, diálogos y objetivos son *ScriptableObjects* (≈ archivos de
   configuración que se editan en el Inspector). Afinar el juego no pide tocar
   código.
4. **El mundo se hace en el editor** ([`ARQUITECTURA.md`](ARQUITECTURA.md) §1).
5. **Presupuesto de rendimiento en cada fase**, medido en el HONOR
   ([`RENDIMIENTO.md`](RENDIMIENTO.md)). Se recorta en la fase en que aparece,
   no al final.
6. **Pruebas automáticas donde hay cuentas**: daño, combos, guardado, objetivos.
   En Godot, un objetivo imposible de completar bloqueó el nivel entero y solo
   se vio jugando (trampa 97).

Las estimaciones van en **sesiones** (una tarde de trabajo con agentes). Son
orientativas: el Nivel 1 de Godot, con todo, fueron unas 17.

## Resumen

| Fase | Qué | Sesiones | Estado |
|---|---|---|---|
| **0** | Cimientos, Serena moviéndose, medidor | — | ✅ |
| **0b** | Medir en el HONOR | 1 | ⏳ cuando esté el teléfono |
| **1** | Serena completa: movimiento avanzado y combate | 4–5 | ⬜ |
| **2** | Enemigos, director de combate y el guardián | 4–5 | ⬜ |
| **3** | El marco del juego: arranque, menús, guardado, audio | 3–4 | ⬜ |
| **4** | Nivel 1 hecho a mano, con todo lo que tenía en Godot | 6–8 | ⬜ |
| **5** | Historia y presentación: apertura, diálogos, mapa de escenarios | 3–4 | ⬜ |
| **6** | Niveles 2, 3 y 4 con sus Sailor jugables | 12–15 | ⬜ |
| **7** | El final: la Torre y la Dama del Espejo | 4–5 | ⬜ |
| **8** | Ampliación (opcional): niveles 5–7, armario, modo asistido | — | ⬜ |

**Hito jugable para la niña:** al cerrar la fase 4 hay un Nivel 1 completo,
igual o mejor que el de Godot. **Juego completo con historia cerrada:** fase 7.

---

## Fase 0b — Medir en el teléfono

La prueba que confirma la decisión con números. Todo está listo
([`RENDIMIENTO.md`](RENDIMIENTO.md) §Cómo se mide): instalar el APK, lanzar el
banco por adb, rellenar la tabla. No bloquea las demás fases, pero **si el
terreno iluminado sale caro, la fase 4 empieza por el shader toon de terreno**.

---

## Fase 1 — Serena completa

Todo lo que hace la jugadora, en una escena de pruebas
(`Scenes/Sandbox/CombatSandbox`) con muñecos de entrenamiento.

### 1.1 Movimiento avanzado
- Doble salto con su voltereta (`double_jump`, recortado como los demás).
- Agarrarse a cornisas y trepar: detección con rayos contra una capa
  «trepable», manos pegadas al canto con **Animation Rigging** (IK). En Godot
  hizo falta un modificador de esqueleto a mano (trampas 88 y 89).
- Ajuste fino de salto, aceleración y cámara jugando en el teléfono.

### 1.2 Combate
- **Ataques como datos** (`AttackData`: daño, ventanas de golpe, forma de la
  caja, empuje, clip, velocidad). Los valores ya afinados vienen de
  [`diseno/05-COMBATE.md`](diseno/05-COMBATE.md).
- **Combo de 3 golpes** con ventana de 0,8 s (se machaca y siempre sale).
- **Auto-orientación** al enemigo más cercano en un cono de 120° y 4 m.
- **La caja de golpe se enciende en el fotograma exacto del impacto** con
  *Animation Events*. En Godot quedó pendiente y el golpe iba por tiempo.
- Cajas de golpe y de daño por capas de física, con **rayo de línea de vista**:
  no se pega a través de paredes (trampa 108).
- **Sensación de impacto** (`CombatFeel`): parada de impacto, sacudida con
  *Cinemachine Impulse*, partículas de estrellas y corazones, destello, sonido
  con tono aleatorio ±10 %.
- **Especial**: barra que se llena golpeando, zoom, invulnerable, área amplia;
  no se recarga a sí mismo.
- **Transformación**: secuencia con **Timeline** (cámara, partículas, música).

### 1.3 Vida
- 5 corazones, 1,5 s de invulnerabilidad con parpadeo tras un golpe.
- Sin *game over*: se marea y reaparece en la misma arena con la vida llena.

**Hecho cuando:** en el teléfono, se juega el combo contra muñecos, se nota cada
golpe, el especial y la transformación se ven bien, y hay pruebas de las
cuentas del combo y del daño.

---

## Fase 2 — Enemigos y el guardián

- **Plantilla de enemigo** con datos (`EnemyData`) y máquina de estados en C#
  con límite de cambios por fotograma (trampa 99).
- **Navegación con NavMesh**: rodean obstáculos de verdad. En Godot iban en
  línea recta.
- **Peluchín** (cuerpo a cuerpo), **Hadita** (a distancia, huye), **Cofrecito**
  (escudo frontal de 130°): animación procedural (rebote, estirar y encoger).
- **Director de combate**: máximo 4 atacando a la vez con fichas; los demás
  esperan en corro.
- **Reglas de compasión**: todo ataque avisado con brillo rojo 0,75 s; nunca por
  la espalda sin avisar.
- **Derrota**: sale despedido, gira, se encoge y estalla en estrellas.
- **Corazones** que sueltan al caer.
- **El guardián del portal**: su modelo con 11 animaciones, 5 ataques, 3 fases,
  aguante contra el encadenado y barra de vida
  ([`diseno/20-JEFE-GUARDIAN.md`](diseno/20-JEFE-GUARDIAN.md)).
- **Oleadas como datos** (`WaveData`) y **arena como prefab**: barrera mágica,
  disparador calculado del radio de la barrera (trampa 91), punto de vuelta
  propio (trampa 104), vigilancia de enemigos perdidos (trampa 95).

**Hecho cuando:** en el teléfono, una arena de tres oleadas y la pelea con el
guardián se juegan enteras, con 8 enemigos y a 60 fps.

---

## Fase 3 — El marco del juego

- **Arranque rápido**: escena `Boot` mínima, **los recursos pesados se cargan
  mientras se ve el menú**, y los shaders se precalientan en la pantalla de
  carga. En Godot eran 15 s de arranque en frío, casi todo de estrenar
  (trampas 130 y 133). **Objetivo: menos de 5 s.**
- **Menú principal, tres partidas guardadas, ajustes, créditos y pausa**
  ([`diseno/23-MENUS.md`](diseno/23-MENUS.md)).
- **Guardado** en JSON con versión, en el almacenamiento de la app.
  Instalar encima no borra la partida (misma firma siempre).
- **Pantalla de carga** que no miente: la quita el nivel cuando está listo.
- **Audio**: *AudioMixer* con grupos (música, efectos, voces, ambiente) y el
  **director de música por capas** con prioridad
  ([`diseno/21-SONIDO-Y-MUSICA.md`](diseno/21-SONIDO-Y-MUSICA.md)). Fundidos
  que no se pisan (trampa 101).
- **Modo desarrollador**: panel (ya existe), saltar al jefe, invencible, recargar.

**Hecho cuando:** se abre el juego, se elige partida, se juega la escena de
combate, se sale, se vuelve y todo sigue donde estaba.

---

## Fase 4 — Nivel 1: el Parque Juuban, hecho a mano

El recorrido de Godot como guía, **no como plano**: placita de arranque →
pasillo → Arena 1 → cruce con rama de plataformas → Arena 2 → cuesta con
faroles y Mercury → guardián → paseo de vuelta → claro del portal. Se rediseña
libremente en el editor ([`ESCENARIOS.md`](ESCENARIOS.md)).

- **Primero en gris** (ProBuilder + Terrain) y jugado en el teléfono antes de
  decorar: ¿se entiende a dónde ir?, ¿las arenas tienen buen tamaño?
- **Terreno toon con luz horneada** (arregla U5): los árboles vuelven a dejar
  sombra en el suelo, sin coste por fotograma.
- Estanque con agua animada y pasarela.
- **Objetivos como datos** que se cuentan solos a partir de lo colocado en la
  escena (trampa 97): 7 Estrellas, faroles, rincones con baliza, arenas.
- Panel de objetivos con flecha guía; **avisos en las puertas** que dicen qué falta.
- Repechos trepables con premio.
- **Luna** acompañando (NavMesh, cola con física) y **Mercury** con su encuentro.
- **Portal** y su cinemática con Timeline.
- Luz horneada, sondas, oclusión horneada.
- Sonido de ambiente con posición y la música del parque.

**Hecho cuando:** el Nivel 1 se juega de principio a fin en el teléfono, a 60
fps en el sitio más cargado, **y la niña lo ha probado.**

---

## Fase 5 — Historia y presentación

- **Sistema de diálogos** con datos: personaje, retrato, expresión, voz,
  animación de hablar. Se avanza tocando en cualquier parte (trampa 34).
- **La apertura**: las cinco viñetas del guion, con Timeline.
- **Mapa de escenarios**: a dónde lleva el portal, qué se lleva la jugadora de
  un nivel a otro y cómo se guarda. Era lo que le faltaba a Godot.
- Escenas entre niveles.
- Voces ([`diseno/14-VOCES.md`](diseno/14-VOCES.md)) y la música que falta
  ([`diseno/24-BANDA-SONORA.md`](diseno/24-BANDA-SONORA.md)).

---

## Fase 6 — Niveles 2, 3 y 4

Cada nivel es una escena nueva, un kit de decorado, una variante de enemigos y
**una Sailor jugable nueva** (mismo esqueleto y animaciones, distinto poder).

| Nivel | Escenario | Enseña | Se une |
|---|---|---|---|
| 2 | Templo Hikawa, atardecer | Enemigos a distancia, cubrirse | **Mars** (fuego, área) |
| 3 | Escuela Juuban | Escudos: rodear o romper | **Jupiter** (rayo, rompe escudos) |
| 4 | Arcade Crown, neón | Grupos rápidos, alcance | **Venus** (cadena, alcance largo) y Artemis |

- **Cambio de Sailor** con menú radial.
- **Poderes como datos** (`SailorData`: combo, especial, efectos).
- Por nivel: kit de props (Blender o hi3d.ai → [`PIPELINE-ASSETS.md`](PIPELINE-ASSETS.md)),
  música, diálogos del guion ([`diseno/16-GUION.md`](diseno/16-GUION.md)) y su guardián.

---

## Fase 7 — El final

- **La Torre del Espejo Roto**: recorrido de arenas con todos los enemigos y
  cambio de Sailor.
- **La Dama del Espejo**: tres fases, cada una vulnerable al poder de una Sailor.
- Final, créditos y **post-créditos con Chibi Moon** (con el nombre de la prima,
  si se quiere).

---

## Fase 8 — Ampliación (opcional)

Niveles 5–7 (Teatro con Tuxedo Mask; Puerto con Neptuno y Uranus; Puerta del
Tiempo con Pluto y Saturno), el **armario** (lazos y colores con las chispas) y
el **modo asistido**. El guion está pensado para poder parar en el nivel 4 sin
que se note ([`diseno/16-GUION.md`](diseno/16-GUION.md) §Lo mínimo y lo deseable).

---

## Decisiones técnicas

| Tema | Decisión | Por qué |
|---|---|---|
| Motor | Unity 6000.6 → **6.7 LTS** cuando salga | Soporte largo; salto pequeño dentro de Unity 6 |
| Render | URP, un solo perfil, toon propio | Medido en Godot: lo caro es el píxel |
| Animación | Animator + Humanoid, clips compartidos por las 11 | Una animación vale para todas |
| Cinemáticas | Timeline + Cinemachine | Se editan en una línea de tiempo, no en código |
| IA de enemigos | FSM en C# + NavMesh | Simple, depurable, rodea obstáculos |
| Interfaz | uGUI + TextMeshPro | Probado en móvil y táctil; UI Toolkit no aporta aquí |
| Datos de juego | ScriptableObjects | Se afinan en el Inspector |
| Guardado | JSON con versión | Legible y migrable |
| Entrada | Input System con `InputReader` único | Teclado, mando y táctil iguales |

## Riesgos

| Riesgo | Qué se hace |
|---|---|
| El terreno de URP no hornea (U5) | Shader toon de terreno propio en la fase 4 |
| El rendimiento en el HONOR no llega | Medir en cada fase; FSR como palanca guardada |
| Clips de Mixamo que no hacen lo que dicen (U11) | Medir cada clip nuevo antes de usarlo |
| El alcance crece | El núcleo son los niveles 1–4 y el final; lo demás es la fase 8 |
| MCP en beta o con cambios | Versión fijada (10.2.0); `tools/unity_mcp.py` como alternativa |
