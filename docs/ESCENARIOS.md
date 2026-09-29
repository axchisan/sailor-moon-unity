# Cómo se construye un escenario

Todo a mano, en el editor. Esta es la receta; la escena de prueba
(`Scenes/Sandbox/PerfTest.unity`) sirve de ejemplo de cada paso.

## 1. El suelo: Unity Terrain

1. *GameObject ▸ 3D Object ▸ Terrain*. Tamaño en el Inspector (pestaña de
   ajustes): la escena de prueba usa 240×240 m y 40 m de alto, heightmap de 257.
2. **Esculpir** (pestaña de pinceles): *Raise or Lower*, *Smooth*, *Set Height*
   para las zonas llanas donde va a haber arenas o caminos. El paquete Terrain
   Tools añade más pinceles (erosión, ruido, terrazas).
3. **Pintar** (*Paint Texture*): las capas son las de
   `Art/Terrain/Surfaces/` (hierba, hierba lejana, camino, roca). Una capa
   nueva = una receta nueva (clic derecho ▸ *Create ▸ Sailor Moon ▸ Superficie
   de terreno*, retocar colores, **Hornear**).
4. Ajustes que ya se probaron: `Pixel Error` 8, `Basemap Distance` 90, `Draw
   Instanced` activado, **sin sombra propia** (`Cast Shadows: Off`).

> Hasta que se arregle U5 ([`TRAMPAS-UNITY.md`](TRAMPAS-UNITY.md)), el terreno
> NO se marca como *Contribute GI*.

## 2. El recorrido

Por dónde se juega: caminos, arenas y claros. Se pinta con la capa de camino y
se aplana con *Set Height*. Para piezas con volumen (plataformas, escaleras,
muros de arena) está **ProBuilder** (*Tools ▸ ProBuilder*): se modela en la
escena y se convierte en prefab cuando esté bien.

Límites del mundo: una colisión invisible (cajas con `BoxCollider` sin
`MeshRenderer`) más alta que lo que se ve — en Godot los setos bajos se
saltaban y la solución buena fue que la colisión subiera más que la malla.

## 3. El decorado

- Arrastrar prefabs de `Prefabs/Props/` a la escena. Ya vienen estáticos, con
  su sombra y su colisión.
- Para repartir muchos (hierba, piedrecitas), la herramienta de pintar detalles
  del Terrain o el *Polybrush*/*scatter* que se decida; siempre revisando a ojo.
- Lo menudo (hierba, piedras, ramitas, hojas) **sin sombra**: en Godot, el pase
  de sombra llegó a costar el triple que la imagen.
- Agrupar bajo un objeto «Decorado» con hijos por tipo: así el banco de medida
  lo puede apagar entero.

## 4. La luz

1. Un sol: `Directional Light`, modo **Mixed**, sombras suaves.
2. *Window ▸ Rendering ▸ Lighting*: el asset de iluminación de la escena. Modo
   **Subtractive**, lightmapper GPU, resolución 3 texels/m, máximo 1024.
3. **Sondas de luz** (`Light Probe Group`) sobre la zona jugable: es como le
   llega la luz horneada a lo que se mueve.
4. **Hornear**: *Generate Lighting* (o `Sailor Moon ▸ Sandbox ▸ Hornear luz`).
   En el M5 tarda segundos.

## 5. Oclusión

Cuando el escenario tenga muros o edificios que tapen: *Window ▸ Rendering ▸
Occlusion Culling ▸ Bake*. Los objetos grandes van marcados *Occluder Static*,
todo lo estático *Occludee Static* (los prefabs de props ya lo llevan).

## 6. Comprobar

- Consola sin errores ni avisos.
- Panel de rendimiento (F1) en el editor para ver batches y triángulos.
- **APK al teléfono** y medir en el sitio más cargado. Si pasa del presupuesto
  de [`RENDIMIENTO.md`](RENDIMIENTO.md), se recorta ahí antes de seguir.
