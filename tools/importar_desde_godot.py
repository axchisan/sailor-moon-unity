#!/usr/bin/env python3
"""Trae los recursos reutilizables del proyecto de Godot a este.

Es la ÚNICA vía por la que entran assets del proyecto viejo: cada archivo que
llega aquí está en una de las listas de abajo, con su nombre nuevo. Así se sabe
qué vino de dónde, y se puede repetir en otra máquina sin acordarse de nada.

Uso:
  ./tools/importar_desde_godot.py                  # todo
  ./tools/importar_desde_godot.py --solo personajes animaciones
  ./tools/importar_desde_godot.py --godot /otra/ruta/sailor-moon

Lo que NO se trae, a propósito:
  · Las copias PNG que Godot extrajo de cada FBX de animación: eran dieciséis
    veces el mismo dibujo de Serena, 50 MB (trampa 102).
  · Las variantes `_alt` y `_viejo` de las animaciones: Godot usaba las de
    nombre base. Siguen allí si hacen falta.
  · Los .glb sin riguear de las Sailor: la versión buena es el FBX rigueado.
  · Todo el código, las escenas y los shaders: se reescriben, no se copian.

Los .glb se convierten a .fbx con Blender (tools/blender/glb_a_fbx.py).
Es idempotente: lo que ya está y no ha cambiado no se vuelve a tocar.
"""
from __future__ import annotations

import argparse
import filecmp
import shutil
import subprocess
import sys
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent
ART = RAIZ / "Assets/_Project/Art"
AUDIO = RAIZ / "Assets/_Project/Audio"
BLENDER = Path("/Applications/Blender.app/Contents/MacOS/Blender")
CONVERSOR = RAIZ / "tools/blender/glb_a_fbx.py"

# nombre en Godot -> carpeta y nombre en Unity
PERSONAJES_MIXAMO = {
    "serena": "Serena",
    "chibi_moon": "ChibiMoon",
    "sailor_mercury": "SailorMercury",
    "sailor_mars": "SailorMars",
    "sailor_venus": "SailorVenus",
    "sailor_jupiter": "SailorJupiter",
    "sailor_saturno": "SailorSaturn",
    "sailor_uranus": "SailorUranus",
    "sailor_neptuno": "SailorNeptune",
    "sailor_pluto": "SailorPluto",
    "tuxedo_mask": "TuxedoMask",
}

# Objetos que sobran dentro de algún .glb y no deben llegar a Unity.
QUITAR = {
    "luna": ["Icosphere"],     # resto de trabajo en Blender: una esfera de 2 m
    "artemis": ["Icosphere"],
}

# Personajes y enemigos que no pasaron por Mixamo: vienen en .glb.
PERSONAJES_GLB = {
    "luna": "Luna",
    "artemis": "Artemis",
    "guardian_portal": "PortalGuardian",
    "peluchin": "Peluchin",
    "cofrecito": "Cofrecito",
    "hadita": "Hadita",
}

# Las que usaba la librería de Godot (tools/construir_libreria_animaciones.gd).
# Si es bucle o no lo decide Unity al importar (ModelImportRules.cs).
ANIMACIONES_SERENA = [
    "idle", "walk", "run", "jump_start", "jump_loop", "land", "double_jump",
    "ledge_grab", "ledge_climb",
    "attack_1", "attack_2", "attack_3", "special", "hurt", "dizzy", "victory",
    "transform_start", "transform_pose",
    "talk_idle", "talk_gesture", "nod", "shake_head", "point", "surprised", "thinking",
]


def copiar(origen: Path, destino: Path) -> bool:
    """Copia si falta o cambió. Devuelve True si escribió algo."""
    if destino.exists() and filecmp.cmp(origen, destino, shallow=False):
        return False
    destino.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(origen, destino)
    return True


def convertir(glb: Path, fbx: Path, texturas: bool) -> bool:
    if fbx.exists() and fbx.stat().st_mtime >= max(glb.stat().st_mtime, CONVERSOR.stat().st_mtime):
        return False
    orden = [str(BLENDER), "-b", "--factory-startup", "--python", str(CONVERSOR),
             "--", str(glb), str(fbx)]
    if texturas:
        orden.append("--texturas")
    if glb.stem in QUITAR:
        orden += ["--quitar", ",".join(QUITAR[glb.stem])]
    r = subprocess.run(orden, capture_output=True, text=True)
    linea = [l for l in r.stdout.splitlines() if l.startswith("[glb_a_fbx]")]
    if r.returncode != 0 or not fbx.exists():
        print(r.stdout[-2000:], r.stderr[-2000:], file=sys.stderr)
        raise SystemExit(f"Blender no pudo convertir {glb.name}")
    print("   ", linea[0] if linea else fbx.name)
    return True


def personajes(godot: Path) -> int:
    n = 0
    origen = godot / "assets/models/characters"
    for viejo, nuevo in PERSONAJES_MIXAMO.items():
        carpeta = ART / "Characters" / nuevo
        n += copiar(origen / f"{viejo}_rigged.fbx", carpeta / f"{nuevo}.fbx")
        n += copiar(origen / f"{viejo}_rigged_0.png", carpeta / f"{nuevo}_Albedo.png")
    for viejo, nuevo in PERSONAJES_GLB.items():
        n += convertir(origen / f"{viejo}.glb", ART / "Characters" / nuevo / f"{nuevo}.fbx", texturas=True)
    return n


def animaciones(godot: Path) -> int:
    n = 0
    origen = godot / "assets/animations/serena"
    for nombre in ANIMACIONES_SERENA:
        n += copiar(origen / f"{nombre}.fbx", ART / "Animations/Humanoid" / f"{nombre}.fbx")
    return n


def superficies(godot: Path) -> int:
    """Las texturas del kit, una sola vez cada una.

    En Godot cada prop llevaba su copia: `banco_parque_tex_madera.png`,
    `cartel_flechas_tex_madera.png`… 71 archivos que eran 27 texturas. Aquí
    van a Art/Materials/Surfaces/Textures/<superficie>.png, y Unity crea el
    material toon de cada una (SurfaceLibrary.cs) al que se enlazan los props.
    """
    n = 0
    vistas: dict[str, Path] = {}
    for png in sorted((godot / "assets/models/props").glob("*_tex_*.png")):
        superficie = png.stem.split("_tex_", 1)[1]
        if superficie in vistas:
            if not filecmp.cmp(vistas[superficie], png, shallow=False):
                print(f"   ⚠ {png.name} no es igual que {vistas[superficie].name}; se usa la primera")
            continue
        vistas[superficie] = png
        n += copiar(png, ART / "Materials/Surfaces/Textures" / f"{superficie}.png")
    return n


def props(godot: Path) -> int:
    n = 0
    for carpeta in ("props", "pickups"):
        origen = godot / "assets/models" / carpeta
        for glb in sorted(origen.glob("*.glb")):
            nombre = glb.stem
            # Los del kit usan superficies compartidas; los de hi3d.ai
            # (bonsái, coral, espada) traen su propia textura.
            del_kit = any(origen.glob(f"{nombre}_tex_*.png"))
            n += convertir(glb, ART / "Props" / nombre / f"{nombre}.fbx",
                           texturas=not del_kit and carpeta == "props")
    return n


def audio(godot: Path) -> int:
    n = 0
    origen = godot / "assets/audio"
    reparto = {"musica": "Music", "ambiente": "Ambience", "sfx": "SFX", "voces": "Voices"}
    for viejo, nuevo in reparto.items():
        for f in sorted((origen / viejo).rglob("*")):
            if f.suffix.lower() in (".wav", ".mp3", ".ogg"):
                n += copiar(f, AUDIO / nuevo / f.relative_to(origen / viejo))
    if (origen / "LICENCIAS.md").exists():
        n += copiar(origen / "LICENCIAS.md", AUDIO / "LICENCIAS.md")
    return n


def ruido(godot: Path) -> int:
    """El patrón de ruido del que salen todas las superficies del terreno."""
    return copiar(godot / "assets/textures/ruido_superficie.png",
                  ART / "Textures/Noise/surface_noise.png")


def fuentes(godot: Path) -> int:
    n = 0
    for f in ("texto.ttf", "titulo.ttf"):
        n += copiar(godot / "assets/fonts" / f, ART / "Fonts" / f)
    return n


PASOS = {
    "personajes": personajes,
    "animaciones": animaciones,
    "superficies": superficies,
    "props": props,
    "audio": audio,
    "fuentes": fuentes,
    "ruido": ruido,
}


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--godot", type=Path,
                    default=RAIZ.parent / "sailor-moon",
                    help="raíz del proyecto de Godot")
    ap.add_argument("--solo", nargs="+", choices=list(PASOS), help="solo estos pasos")
    args = ap.parse_args()

    if not (args.godot / "project.godot").exists():
        raise SystemExit(f"No encuentro el proyecto de Godot en {args.godot}")
    if not BLENDER.exists():
        raise SystemExit(f"Falta Blender en {BLENDER}")

    for nombre in args.solo or PASOS:
        print(f"· {nombre}")
        cambios = PASOS[nombre](args.godot)
        print(f"  {cambios} archivos nuevos o actualizados")


if __name__ == "__main__":
    main()
