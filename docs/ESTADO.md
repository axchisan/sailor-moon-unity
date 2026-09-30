# ESTADO DEL PROYECTO — empieza por aquí

> **Última actualización:** 2026-09-29 · **Diseño rehecho: ciudad abierta.**
> El juego ya no es una fila de niveles: es Juuban con barrios abiertos, 30
> misiones de la gente, minijuegos, álbum, armario y una historia con lore.
> **Todo en [`juego/00-BIBLIA.md`](juego/00-BIBLIA.md)**; el orden de trabajo, en
> [`juego/09-ROADMAP.md`](juego/09-ROADMAP.md). Lo siguiente: Fase 1 (Serena completa).

## 1. Qué es

Juego **Sailor Moon** en 3D para Android, regalo personal para la prima de 8
años del usuario. **No comercial, no se publica en ninguna tienda.** Beat 'em up
ligero con exploración, cel shading anime, todo en español.

Es la **segunda versión**. La primera, en Godot 4.7 (`../sailor-moon`), llegó a
tener el Nivel 1 completo y jugable, pero con dos problemas de fondo: el mundo
se generaba por código en cada arranque (imposible de editar a mano, imposible
de hornear) y el rendimiento en el teléfono de gama media no llegaba. Aquí se
rehace **en el editor**, reutilizando todos los assets y lo aprendido.

## 2. En qué punto estamos

| Fase | Estado |
|---|---|
| **0 — Cimientos** | ✅ Proyecto, configuración, MCP, importación, documentación |
| **0b — Prueba de rendimiento** | 🟡 Escena y APK listos; **falta medir en el HONOR** |
| 1–8 | ⬜ Ver [`PLAN.md`](PLAN.md) |

### Lo que ya funciona

- **Proyecto Unity 6000.6.3f1 + URP**, configurado por código (`ProjectSetup.cs`)
  para la Adreno 619: Forward, sin HDR, MSAA 4x, sombra de una cascada a 30 m,
  Vulkan, IL2CPP ARM64.
- **MCP** (CoplayDev unity-mcp 10.2.0) funcionando: un agente puede leer la
  consola, ejecutar C# en el editor, sacar capturas, compilar y lanzar builds.
- **Assets importados con reglas automáticas:**
  - 11 Sailor rigueadas como **Humanoid** (las 11 validadas).
  - 25 animaciones de Mixamo con su bucle y la altura de la raíz descartada.
  - Luna, Artemis, el guardián y los tres enemigos, convertidos desde .glb.
  - 41 props con **27 superficies compartidas**.
  - 89 audios y las 2 fuentes.
- **Shader toon propio** para móvil y **cielo en degradado**.
- **Superficies del terreno** como recetas editables que se hornean a capas de Unity Terrain.
- **Serena jugable:**
  - Movimiento relativo a la cámara, salto con coyote time y jump buffer.
  - Animator compartido por las once (locomoción y salto).
  - Física de coletas.
  - Cámara orbital con Cinemachine.
  - Joystick flotante.
- **Escena de prueba** (`Scenes/Sandbox/PerfTest.unity`):
  - Parque de 240 m con camino, lomas y cordillera.
  - ~1.600 props, con cerezos con dos niveles de detalle.
  - Luz horneada en los props y sondas de luz.
- **Aparato de medida:**
  - Panel F1 / tres dedos: fps, p99, CPU y GPU en ms, estado térmico.
  - Banco automático que se lanza por adb e intercala la referencia entre pruebas.

### Fase 1 — en marcha (2026-09-30)

- ✅ **Escala real** del mundo, la cámara y las velocidades ([`ESCALA.md`](ESCALA.md)).
- ✅ **Terreno toon con luz horneada** (Shadowmask): GPU 15,9 → **13,9 ms** en el HONOR.
- ✅ **Combate** ([`COMBATE.md`](COMBATE.md)): combo de 3 sincronizado con la
  animación, auto-orientación e imán, especial con barra, corazones, herida y
  mareo sin *game over*, sensación de impacto. Escena `CombatSandbox`.
- ⬜ Transformación y zoom del especial.
- ⬜ Tiara Lunar (primer poder de campo).
- ⬜ Doble salto y agarrar cornisas.
- ⬜ Afinar el combate jugando en el teléfono.

## 3. Documentos

| Documento | Qué tiene |
|---|---|
| [`juego/00-BIBLIA.md`](juego/00-BIBLIA.md) | ⭐⭐ **El diseño del juego**: visión, estructura, índice de lore, mundo, jugabilidad, historia, personajes, misiones, actividades, contenido y roadmap |
| [`juego/09-ROADMAP.md`](juego/09-ROADMAP.md) | ⭐ **El orden de trabajo**: fases, hitos para la prima, carriles de producción |
| [`PLAN.md`](PLAN.md) | Plan anterior (niveles en fila), sustituido por el roadmap |
| [`CONTROLES.md`](CONTROLES.md) | Controles de PC, mando y móvil; cómo jugar en el Mac |
| [`../AGENTS.md`](../AGENTS.md) | ⭐ Reglas del proyecto y cómo trabajar con Unity desde un agente |
| [`ARQUITECTURA.md`](ARQUITECTURA.md) | ⭐ Por qué el mundo se hace en el editor; piezas, carpetas, código, render |
| [`RENDIMIENTO.md`](RENDIMIENTO.md) | ⭐ Presupuestos, cómo se mide en el teléfono, decisiones de render y resultados |
| [`PIPELINE-ASSETS.md`](PIPELINE-ASSETS.md) | Cómo entra cada tipo de asset y qué hace Unity con él |
| [`ESCENARIOS.md`](ESCENARIOS.md) | Receta para construir un nivel en el editor |
| [`COMBATE.md`](COMBATE.md) | ⭐ **El combate**: cómo está hecho y «quiero cambiar X → toca aquí» |
| [`ESCALA.md`](ESCALA.md) | ⭐ **El estándar de tamaños**: cuánto mide cada cosa, la cámara, las velocidades y cómo entra un modelo nuevo |
| [`TRAMPAS-UNITY.md`](TRAMPAS-UNITY.md) | ⭐ Fallos ya pisados en este proyecto |
| [`aprendido/LECCIONES-GODOT.md`](aprendido/LECCIONES-GODOT.md) | ⭐ Las 133 trampas de Godot, destiladas a lo que sigue valiendo |
| [`diseno/`](diseno/) | Diseño heredado de Godot. **Manda `juego/` si se contradicen**; sigue valiendo para los números del combate, el audio y el pipeline |

## 4. Nota legal

Sailor Moon es propiedad de Naoko Takeuchi / Kodansha / Toei Animation. Regalo
personal, no distribuido ni monetizado. **No publicar en Google Play, itch.io
ni ninguna tienda.** Instalación por sideload en el teléfono de la prima.
