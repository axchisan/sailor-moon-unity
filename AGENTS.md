# Instrucciones para agentes — Sailor Moon (Unity)

> Léelo entero antes de tocar nada. Después, [`docs/ESTADO.md`](docs/ESTADO.md)
> y, para cualquier cosa de diseño, historia o contenido,
> [`docs/juego/00-BIBLIA.md`](docs/juego/00-BIBLIA.md) (manda sobre todo lo demás).

## Qué es

Juego 3D de Sailor Moon para Android: un regalo personal para una niña de 8
años. **No comercial, no se publica en ninguna tienda.** Es la segunda versión:
la primera se hizo en Godot (`../sailor-moon`) y se rehace en Unity para
trabajar **desde el editor** y no desde código que genera el mundo al arrancar.

**El usuario** es desarrollador backend (Java/Spring) con poca experiencia en
gamedev y ninguna en 3D. Explica los términos de 3D la primera vez y usa
analogías de backend: prefab ≈ plantilla/clase, instancia de prefab ≈ objeto,
ScriptableObject ≈ DTO/config persistida, escena ≈ documento, componente ≈
dependencia inyectada.

## Reglas del proyecto

1. **El editor manda.** Los niveles, los prefabs y los materiales son assets
   que se editan en Unity. Un script puede montar un andamio UNA vez (menú
   `Sailor Moon ▸ …`), pero el resultado se guarda y se edita a mano. **Nada se
   genera al arrancar el juego.** Ver [`docs/ARQUITECTURA.md`](docs/ARQUITECTURA.md) §1.
2. **Código en inglés; comentarios, documentación, commits y textos del juego en
   español.** Los comentarios explican el POR QUÉ, con referencia a la trampa o
   la medida que lo justifica.
3. **Medir en el teléfono, nunca en el Mac.** El Mac va a 60 fps con todo y no
   dice nada. Ver [`docs/RENDIMIENTO.md`](docs/RENDIMIENTO.md).
4. **Configuración como código.** Ajustes del proyecto en
   `ProjectSetup.cs`, reglas de importación en `ModelImportRules.cs`. Nadie
   toca un importador o un ajuste a mano sin reflejarlo ahí.
5. **Los assets del proyecto de Godot entran solo por**
   `tools/importar_desde_godot.py`.
6. **Antes de dar algo por hecho, verifícalo** en el editor (consola sin
   errores, captura) o en el teléfono.
7. **Al cerrar una fase, APK al teléfono** para que el usuario lo pruebe.

## Trabajar con Unity desde un agente (MCP)

El servidor MCP es **CoplayDev unity-mcp v10.2.0** (MIT), fijado en
`Packages/manifest.json` y en [`.mcp.json`](.mcp.json). Transporte **stdio**,
sin telemetría ([`McpDefaults.cs`](Assets/_Project/Scripts/Editor/McpDefaults.cs)).

- **Unity tiene que estar abierto** con este proyecto: el puente escucha en el
  `127.0.0.1:6400`. Comprobar: `lsof -nP -iTCP:6400 -sTCP:LISTEN`.
- Una sesión de Claude Code abierta EN ESTA CARPETA carga el MCP sola
  (`mcp__unity__*`). Desde fuera, o para scripts: `tools/unity_mcp.py`.

```bash
./tools/unity_mcp.py herramientas                       # qué hay
./tools/unity_mcp.py codigo archivo.cs                  # ejecutar C# en el editor
echo 'return Application.version;' | ./tools/unity_mcp.py codigo -
./tools/unity_mcp.py llamar read_console '{"action":"get","count":20,"types":["error"]}'
./tools/unity_mcp.py llamar manage_camera '{"action":"screenshot","camera":"Main Camera","output_folder":"Captures"}'
```

- Tras escribir scripts, `refresh_unity` y **esperar**: mientras Unity recompila
  el puente se corta unos segundos («Connection closed»). Reintentar, no asumir
  fallo. Comprobar con `UnityEditor.EditorUtility.scriptCompilationFailed`.
- **Con el editor abierto, NUNCA mover ni renombrar scripts con `mv`**: usar
  `AssetDatabase.MoveAsset`. Ver [`docs/TRAMPAS-UNITY.md`](docs/TRAMPAS-UNITY.md) U1.
- **Nada de diálogos modales** en herramientas de editor: bloquean el editor y
  al MCP con él (U4).
- **Con el editor abierto no se puede usar `-batchmode`** sobre el proyecto
  (está bloqueado). Con el editor cerrado, sí:
  ```bash
  /Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity \
    -batchmode -nographics -projectPath . -executeMethod <Clase.Metodo> -quit -logFile -
  ```

## Entorno

| Cosa | Valor |
|---|---|
| Unity | **6000.6.3f1** (Apple silicon), con Android (JDK, SDK y NDK propios de Unity). Subir a **6.7 LTS** cuando salga |
| Render | URP 17.6, un único asset `Settings/Rendering/URP_Mobile.asset` |
| Android | `com.familia.sailormoon.unity`, IL2CPP, ARM64, minSdk 26, Vulkan + GLES3 |
| Teléfono de medida | HONOR LGN-NX3 · Adreno 619 · 1610×720 · Android 16 |
| adb | `/opt/homebrew/share/android-commandlinetools/platform-tools/adb` |
| Blender | `/Applications/Blender.app` (5.2) — conversión de .glb, ver `tools/blender/` |
| git-lfs | `export PATH="/opt/homebrew/bin:$PATH"` antes de `git add` en shells no interactivas |

## Mapa rápido

```
Assets/_Project/
  Art/            modelos, animaciones, texturas, shaders, superficies
  Prefabs/        personajes, props, gameplay
  Scenes/         Boot, niveles, Sandbox (pruebas)
  Settings/       URP, entrada
  Scripts/Runtime SailorMoon.Runtime (el juego)
  Scripts/Editor  SailorMoon.Editor (importación, montaje, configuración)
tools/            importar_desde_godot.py, unity_mcp.py, blender/
docs/             ESTADO.md primero
```
