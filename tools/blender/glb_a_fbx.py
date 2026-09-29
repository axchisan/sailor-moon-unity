"""Convierte .glb a .fbx para Unity, con sus texturas como archivos al lado.

POR QUÉ: Unity no importa glTF de serie, y el FBX es su formato de primera:
genera UV de lightmap, extrae clips de animación y admite LOD por nombre
(`_LOD0`, `_LOD1`). Los kits de props y los personajes que no pasaron por
Mixamo (Luna, Artemis, el guardián, los enemigos) salieron de Blender en .glb
para Godot; aquí se pasan una vez y ya.

Uso (lo llama tools/importar_desde_godot.py):
  Blender -b --factory-startup --python glb_a_fbx.py -- entrada.glb salida.fbx
          [--texturas] [--quitar Objeto1,Objeto2]

  --texturas  saca las imágenes empaquetadas en el .glb como PNG junto al FBX.
              Solo para modelos con textura propia (personajes, props de
              hi3d.ai). Los del kit usan las superficies compartidas de
              Art/Materials/Surfaces y no deben llevar copia.
  --quitar    borra objetos que sobran antes de exportar (restos de trabajo
              en Blender, como la Icosphere de 2 m que traía el .glb de Luna).

Ejes: se exporta con los ejes por defecto del FBX de Blender (-Z adelante,
Y arriba) y el importador de Unity activa `bakeAxisConversion`. Así el frente
del modelo queda en +Z, que en Unity es «adelante»: se acaba el giro de 180°
que había que darle a todo en Godot (trampas 12 y 14).
"""
import os
import sys

import bpy


def main():
    argv = sys.argv[sys.argv.index("--") + 1:]
    entrada, salida = argv[0], argv[1]
    con_texturas = "--texturas" in argv
    quitar = argv[argv.index("--quitar") + 1].split(",") if "--quitar" in argv else []

    bpy.ops.wm.read_homefile(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=entrada)

    for nombre in quitar:
        ob = bpy.data.objects.get(nombre)
        if ob is None:
            raise SystemExit(f"--quitar: no existe el objeto {nombre} en {entrada}")
        bpy.data.objects.remove(ob, do_unlink=True)

    carpeta = os.path.dirname(salida)
    os.makedirs(carpeta, exist_ok=True)
    base = os.path.splitext(os.path.basename(salida))[0]
    for img in bpy.data.images:
        if not img.packed_file:
            continue
        if con_texturas:
            img.filepath_raw = os.path.join(carpeta, f"{base}_{img.name}.png")
            img.file_format = "PNG"
            img.save()
        else:
            # Sin textura propia: el material se enlaza por NOMBRE a la
            # superficie compartida al importar en Unity (ModelImportRules).
            img.filepath_raw = f"{img.name}.png"

    tiene_animacion = len(bpy.data.actions) > 0

    bpy.ops.export_scene.fbx(
        filepath=salida,
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        bake_space_transform=False,
        object_types={"ARMATURE", "MESH", "EMPTY"},
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        bake_anim=tiene_animacion,
        bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False,
        bake_anim_simplify_factor=0.0,
        # Las texturas van como archivos al lado (o en la biblioteca de
        # superficies); el FBX solo guarda el nombre.
        path_mode="STRIP",
        embed_textures=False,
    )

    mallas = [o for o in bpy.data.objects if o.type == "MESH"]
    tris = sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in mallas)
    print(f"[glb_a_fbx] {os.path.basename(salida)}: {len(mallas)} mallas, "
          f"{tris} triángulos, {len(bpy.data.actions)} animaciones")


main()
