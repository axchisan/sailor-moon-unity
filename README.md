# Sailor Moon — El Espejo de los Sueños (Unity)

Juego 3D de Sailor Moon para Android: beat 'em up ligero con exploración y
cel shading anime. **Regalo personal para una niña de 8 años. No comercial;
no se distribuye ni se publica en ninguna tienda.** Sailor Moon es propiedad de
Naoko Takeuchi / Kodansha / Toei Animation.

Segunda versión del proyecto, rehecha en Unity a partir de la primera en Godot.

## Empezar

- **Documentación:** [`docs/ESTADO.md`](docs/ESTADO.md) primero.
- **Agentes:** [`AGENTS.md`](AGENTS.md).

## Requisitos

- Unity **6000.6.3f1** con Android Build Support (incluye JDK, SDK y NDK).
- `git-lfs` (modelos, texturas y audio van por LFS).
- Para traer assets del proyecto de Godot: Blender 5.2 y Python 3.
- Para el MCP: `uv` (`brew install uv`).

```bash
git lfs install
git clone https://github.com/axchisan/sailor-moon-unity.git
# Abrir la carpeta con Unity Hub (versión 6000.6.3f1)
# Menú Sailor Moon ▸ Configurar proyecto   (idempotente)
```

## Probar en el teléfono

```bash
ADB=/opt/homebrew/share/android-commandlinetools/platform-tools/adb
$ADB install -r export/sailor_moon_unity_dev.apk
```

La escena de prueba es `Assets/_Project/Scenes/Sandbox/PerfTest.unity`.
Controles: WASD o joystick en pantalla, ratón con botón derecho o arrastrar a
la derecha para mirar, espacio o botón para saltar. F1 (o tres dedos) para el
panel de rendimiento.
