# Arquitectura

Cómo está montado el proyecto y, sobre todo, **por qué es distinto del de
Godot**. Si solo lees una sección, lee la 1.

## 1. El cambio de fondo: el mundo se hace en el editor

En Godot **todo el Nivel 1 se generaba por código en cada arranque**: el
terreno por ruido, 1.860 props sembrados con rayos, los muros, los senderos y
las arenas. Eso tuvo tres consecuencias, y las tres salieron caras:

1. **No se podía editar.** Mover un árbol era cambiar una constante y rezar para
   que la siembra no se reordenara. La mitad de las trampas de Godot (§6 de su
   ESTADO.md, de la 29 a la 83) son fallos de colocación automática: props
   enterrados, muros flotando, el estanque debajo del parque, bancos dentro de
   peñascos…
2. **No se podía hornear nada.** Luz, sombras, oclusión o agrupado de objetos
   estáticos necesitan que el mundo exista ANTES de jugar. Un mundo que nace al
   arrancar solo puede iluminarse en tiempo real, y en la Adreno 619 iluminar
   el suelo era el cuello de botella medido.
3. **Arrancar costaba.** Parte de los 15 s de carga en el teléfono era montar
   el parque.

Aquí la regla es la contraria:

> **Un nivel es una escena de Unity guardada en disco.** Se esculpe, se pinta
> y se decora a mano con las herramientas del editor. Al jugar se carga, no se
> construye.

Se permite —y se usa— código de editor que monte un **andamio** (el menú
`Sailor Moon ▸ …`): la escena de prueba, un prefab, el Animator. Pero se
ejecuta UNA vez, deja assets normales y a partir de ahí se trabaja a mano. Si
un andamio se vuelve a ejecutar, pisa lo hecho a mano: por eso lo que sale de
él se trata como punto de partida, no como fuente de verdad.

## 2. Piezas y su equivalente en Godot

| Unity | Para qué | En Godot era |
|---|---|---|
| **Escena** (`.unity`) | Un nivel, un menú, una prueba | `.tscn` + `nivel.gd` que lo generaba |
| **Prefab** (`.prefab`) | Plantilla reutilizable: Serena, un cerezo, un farol | Escena instanciada |
| **Componente** (`MonoBehaviour`) | Comportamiento pegado a un objeto | Script en un nodo |
| **ScriptableObject** | Datos editables en el Inspector (≈ DTO persistido) | `Resource` |
| **Animator Controller** | Máquina de estados de animación, editable en su ventana | `AnimationTree` |
| **Cinemachine** | Cámara orbital con colisión y encuadre | `SpringArm3D` escrito a mano |
| **Unity Terrain** | Suelo esculpido y pintado con pinceles | `terreno_nivel.gd` por ruido |
| **Lightmaps** | Luz y sombras de lo estático, horneadas | No había: todo en tiempo real |
| **Input System** | Acciones con nombre (mover, saltar) | Input Map |

## 3. Carpetas

```
Assets/_Project/
  Art/
    Characters/<Nombre>/     FBX + textura + material toon del personaje
    Animations/Humanoid/     clips de Mixamo (compartidos por las 11)
    Animations/SailorLocomotion.controller
    Props/<nombre>/          FBX del decorado (convertidos de .glb)
    Materials/Surfaces/      superficies compartidas: madera, piedra, hoja…
    Terrain/Surfaces/        recetas de superficie del terreno y sus capas
    Shaders/                 Toon.shader, SkyGradient.shader
  Audio/Music|Ambience|SFX|Voices
  Prefabs/Characters|Props|Enemies|Gameplay
  Scenes/Boot|Sandbox|...
  Settings/Rendering         URP_Mobile.asset y su renderer
  Scripts/Runtime/           SailorMoon.Runtime.asmdef — el juego
  Scripts/Editor/            SailorMoon.Editor.asmdef — herramientas de editor
  Tests/EditMode|PlayMode
```

Todo lo nuestro vive bajo `_Project/`: lo que está fuera es de paquetes o de
Unity. Las dos asmdef separan el juego de las herramientas: el código de editor
no puede colarse en la build, y cambiar una herramienta no recompila el juego.

## 4. Código

- **Namespaces** por carpeta: `SailorMoon.Player`, `SailorMoon.CameraRig`,
  `SailorMoon.UI`, `SailorMoon.Diagnostics` (la carpeta es `Debug/`, pero el
  namespace no se llama así para no tapar `UnityEngine.Debug`).
- **Campos privados serializados** con `[SerializeField] _camelCase` y
  `[Tooltip]` en español: el Inspector es la interfaz del diseñador.
- **Un componente, un trabajo.** `InputReader` lee la entrada, `PlayerMotor`
  mueve, `PlayerAnimator` pasa el estado al Animator. Ninguno sabe cómo lo hace
  el otro.
- **La entrada entra por un solo sitio** (`InputReader`): teclado, mando y
  táctil escriben ahí. Nadie lee una tecla directamente.
- **Orden de ejecución explícito** donde importa: `TouchControls` (-200) →
  `InputReader` (-100) → resto → `SpringBoneChain` (+100, después del Animator).

## 5. Configuración como código

| Qué | Dónde | Cómo se reaplica |
|---|---|---|
| Player, Android, URP, calidad | `Scripts/Editor/ProjectSetup.cs` | `Sailor Moon ▸ Configurar proyecto` |
| Importación de modelos, clips, texturas, audio | `Scripts/Editor/Import/ModelImportRules.cs` | Automático; subir `Version` fuerza reimportar |
| Superficies compartidas del decorado | `Scripts/Editor/Import/SurfaceLibrary.cs` | Automático al llegar una textura |
| Puente MCP | `Scripts/Editor/McpDefaults.cs` | Automático al abrir |

## 6. Render

- **URP** con un único asset para todas las plataformas (`URP_Mobile`). Lo que
  se ve en el Mac es lo que se ve en el teléfono: en Godot, el perfil de móvil
  cambiaba el aspecto sin que nadie lo decidiera (trampa 114).
- **Forward**, sin HDR, MSAA 4x, sombra de una cascada a 30 m, luces extra por
  vértice. Cada ajuste lleva su motivo en `ProjectSetup.cs`.
- **`SailorMoon/Toon`** para personajes y props: dos tonos, sombra recibida en
  el mismo escalón, ambiente por vértice, lightmap para lo estático, rim que
  pasa por la luz y contorno por casco invertido (solo personajes).
- **Iluminación Subtractive**: lo estático lleva la luz horneada; en tiempo real
  solo la sombra de lo que se mueve.
- **Terreno**: Unity Terrain con el shader de URP, **sin lightmap** de momento
  (fallo de Unity 6000.6, ver [`TRAMPAS-UNITY.md`](TRAMPAS-UNITY.md) U5).
