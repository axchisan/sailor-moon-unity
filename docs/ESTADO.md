# ESTADO DEL PROYECTO — empieza por aquí

> **Última actualización:** 2026-09-29 · **Cimientos montados.** Proyecto de
> Unity creado, assets de Godot traídos, escena de prueba jugable y APK de
> desarrollo compilado. **Falta medirlo en el teléfono**, que es la pregunta
> que decide si el remake sigue adelante.

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
| 1 — Serena completa (movimiento, combate, transformación) | ⬜ |
| 2 — Nivel 1 hecho a mano | ⬜ |
| 3+ — Resto de escenarios e historia | ⬜ |

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

### Lo siguiente, en orden

1. **Medir en el HONOR** (`RENDIMIENTO.md` §Cómo se mide) y rellenar la tabla.
   Es la decisión: si Unity no rinde claramente mejor que Godot en el mismo
   sitio, se vuelve a pensar.
2. **Jugarlo en el teléfono**: que el joystick, la cámara y el salto se sientan
   bien a los 8 años.
3. **Terreno con luz horneada** (U5): probar en 6.7 LTS o hacer el shader toon
   de terreno. Hoy los árboles no dejan sombra horneada en el suelo.
4. **Fase 1 — Serena completa:** combo de 3 golpes, especial, transformación,
   con el ajuste de `diseno/05-COMBATE.md`.
5. **Nivel 1 a mano**, siguiendo `ESCENARIOS.md`.

## 3. Documentos

| Documento | Qué tiene |
|---|---|
| [`../AGENTS.md`](../AGENTS.md) | ⭐ Reglas del proyecto y cómo trabajar con Unity desde un agente |
| [`ARQUITECTURA.md`](ARQUITECTURA.md) | ⭐ Por qué el mundo se hace en el editor; piezas, carpetas, código, render |
| [`RENDIMIENTO.md`](RENDIMIENTO.md) | ⭐ Presupuestos, cómo se mide en el teléfono, decisiones de render y resultados |
| [`PIPELINE-ASSETS.md`](PIPELINE-ASSETS.md) | Cómo entra cada tipo de asset y qué hace Unity con él |
| [`ESCENARIOS.md`](ESCENARIOS.md) | Receta para construir un nivel en el editor |
| [`TRAMPAS-UNITY.md`](TRAMPAS-UNITY.md) | ⭐ Fallos ya pisados en este proyecto |
| [`aprendido/LECCIONES-GODOT.md`](aprendido/LECCIONES-GODOT.md) | ⭐ Las 133 trampas de Godot, destiladas a lo que sigue valiendo |
| [`diseno/`](diseno/) | Diseño heredado: GDD, guion, reparto, combate, audio, menús, vidas, jefe… |

## 4. Nota legal

Sailor Moon es propiedad de Naoko Takeuchi / Kodansha / Toei Animation. Regalo
personal, no distribuido ni monetizado. **No publicar en Google Play, itch.io
ni ninguna tienda.** Instalación por sideload en el teléfono de la prima.
