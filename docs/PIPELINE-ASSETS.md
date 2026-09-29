# Cómo entran los assets

## Resumen

```
hi3d.ai (~2 M tris, 4K)  ─▶  Blender (reducir, separar, riguear)  ─▶  .fbx  ─▶  Unity
Mixamo (esqueleto + clips) ────────────────────────────────────────▶  .fbx  ─▶  Unity
Proyecto de Godot (.glb) ─▶  tools/importar_desde_godot.py + Blender ─▶ .fbx ─▶  Unity
```

Unity decide cómo importar cada cosa **según la carpeta donde cae**
(`ModelImportRules.cs`). Nadie ajusta un importador a mano: si algo sale mal,
se arregla la regla y vale para todo lo que venga detrás.

## Traer lo del proyecto de Godot

```bash
./tools/importar_desde_godot.py                    # todo
./tools/importar_desde_godot.py --solo props       # un paso
```

Copia personajes, animaciones, superficies, props (convirtiendo .glb → .fbx
con Blender), audio, fuentes y la textura de ruido. Es idempotente. Lo que deja
fuera a propósito está en su cabecera (las 16 copias del PNG de Serena, las
variantes `_alt`, los .glb sin riguear).

## Personajes (Mixamo)

Carpeta `Art/Characters/<Nombre>/`: el FBX rigueado y `<Nombre>_Albedo.png`.

- **Humanoid**, avatar del propio modelo. Las once comparten esqueleto y Unity
  reajusta los mismos clips a cada altura (de 1,10 m Chibi Moon a 1,80 m Tuxedo).
- **Sin materiales del FBX**: el prefab lleva `<Nombre>_Toon.mat`, editable.
- **Jerarquía sin optimizar**: los huesos del pelo (`Hair_L_1…5`,
  `Hair_R_1…5` de Serena) tienen que existir para `SpringBoneChain`.
- Mixamo mira a +Z y Unity también: **no hay que girar nada** (en Godot, 180°
  a todo).

Prefab jugable: `Sailor Moon ▸ Montaje ▸ Prefab de Serena`
(`CharacterBuilder.cs`). Raíz con `CharacterController`, `InputReader`,
`PlayerMotor` y `PlayerAnimator`; dentro, el modelo con su `Animator` y las dos
cadenas de pelo.

## Animaciones (Mixamo)

Carpeta `Art/Animations/Humanoid/`, un FBX por clip, nombre = nombre del clip.
Descarga: FBX Binary, 30 fps, sin reducción de keyframes, **In Place** en todo
menos `idle` (detalle en [`diseno/08-ANIMACIONES-MIXAMO.md`](diseno/08-ANIMACIONES-MIXAMO.md)).

Al importar (`OnPreprocessAnimation`):

- El clip se llama como el archivo, no `mixamo.com`.
- Bucle solo en `idle, walk, run, jump_loop, ledge_grab, dizzy, talk_idle,
  thinking`. Un gesto en bucle parece un tic.
- Giro y avance horizontal horneados en la pose (In Place de verdad).
- **La altura de la raíz NO se hornea y se mide desde los pies.** Con
  `applyRootMotion = false`, lo que la animación suba el cuerpo entero se
  descarta y el salto lo pone la física. En Godot la animación y la física se
  sumaban: Serena subía de más y atravesaba el suelo al aterrizar.

El Animator compartido es `Animations/SailorLocomotion.controller`
(`Sailor Moon ▸ Montaje ▸ Animator de las Sailor`): locomoción en blend tree
(idle / walk / run por `Speed`) y salto (`JumpStart → JumpLoop → Land`). Se
edita en la ventana Animator.

## Props del decorado

Carpeta `Art/Props/<nombre>/`. Sin esqueleto, **UV de lightmap generadas**
(sin ellas no se pueden hornear), material toon sin contorno.

**Superficies compartidas:** los props del kit usan materiales por nombre
(`madera`, `piedra`, `hoja`, `flor`…). Cada uno se enlaza a
`Art/Materials/Surfaces/<nombre>.mat` al importarse. Son 27 superficies para 38
props: retocar «madera» cambia a la vez bancos, vallas, puente y carteles. Los
props de hi3d.ai (bonsái, coral, espada) llevan su textura propia al lado del
FBX.

Prefab: se crea al montar la escena de prueba (`Prefabs/Props/<nombre>.prefab`)
con flags de estático, sombra solo si tiene bulto y colisionador aproximado. El
cerezo es un prefab con **dos niveles de detalle** (`CherryTree`: cerca y lejos).

## Terreno

Las superficies del suelo (hierba, hierba lejana, camino, roca) son **recetas**
(`Art/Terrain/Surfaces/*.asset`, `TerrainSurfaceRecipe`): tres colores y unos
números, los mismos de `paleta_terreno.gd` en Godot. El botón **Hornear** del
Inspector genera la textura y la capa de Unity Terrain. Para cambiar el verde de
la hierba: se retoca la receta, se hornea, y el terreno cambia.

En Godot ese cálculo se hacía en el shader, por píxel y en cada fotograma.
Aquí se hace una vez.

## Audio

`Audio/Music` y `Audio/Ambience` en streaming (no ocupan memoria entera);
`Audio/SFX` y `Audio/Voices` descomprimidos al cargar y en mono. Todo en
Vorbis. Licencias en `Audio/LICENCIAS.md`.
