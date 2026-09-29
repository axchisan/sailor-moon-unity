# Trampas de Unity ya pisadas

Lo que costó tiempo en ESTE proyecto. Las de Godot están destiladas en
[`aprendido/LECCIONES-GODOT.md`](aprendido/LECCIONES-GODOT.md); estas son las
nuevas. Cada una lleva el síntoma, porque es lo que se ve primero.

### U1. Renombrar un script con `mv` y el editor abierto lo deja fuera de la compilación

**Síntoma:** `error CS2001: Source file '…/PlayerInput.cs' could not be found`,
y después `The type or namespace name 'InputReader' could not be found`, aunque
el archivo existe, tiene su `.meta` y Unity lo reconoce como `MonoScript`.

**Causa:** el AssetDatabase guarda la lista de scripts por ensamblado; un
cambio de nombre hecho por fuera, con el editor abierto, puede no llegar a esa
lista aunque el asset nuevo se importe. `Refresh`, `ImportAsset(ForceUpdate)` y
`RequestScriptCompilation` no lo arreglan.

**Arreglo:** mover el script con `AssetDatabase.MoveAsset(origen, destino)`
(ida y vuelta si hace falta). **Regla:** con el editor abierto, los scripts se
mueven y renombran desde Unity o con `AssetDatabase.MoveAsset`, nunca con `mv`.

### U2. El `TerrainData` tiene que existir como asset ANTES de pintarle capas

**Síntoma:** todo el terreno sale con la primera capa, sin error.

**Causa:** las texturas de mezcla de capas son sub-assets del `TerrainData`. Si
se llama a `SetAlphamaps` antes de `AssetDatabase.CreateAsset`, no se guardan.

**Arreglo:** `CreateAsset` primero, después `terrainLayers`, `SetHeights`,
`SetAlphamaps` y `SaveAssets`.

### U3. En Android, Unity va a 30 fps si nadie pide otra cosa

`Application.targetFrameRate` vale -1 y en móvil eso es 30. Sin tocarlo, el
juego va a la mitad y parece un problema de rendimiento que no existe. Lo pone
`GameBootstrap` antes de la primera escena.

### U4. Un diálogo modal en una herramienta de editor cuelga al agente

`EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()` y
`EditorUtility.DisplayDialog` abren una ventana que para el hilo principal del
editor; el MCP deja de responder hasta que alguien la cierre a mano. Las
herramientas de `Scripts/Editor` no preguntan: deciden y lo dejan escrito.

### U5. Terrain de URP + lightmap = terreno en blanco y negro (Unity 6000.6.3)

**Síntoma:** tras hornear, el terreno se ve blanco puro en unas capas y negro
en otras (el camino negro, la hierba blanca), con el borde siguiendo la mezcla
de capas. Sin el lightmap (`terrain.lightmapIndex = -1`) se ve perfecto.

**Descartado, con prueba:** no es la sombra en tiempo real (apagada, igual), ni
la compresión del lightmap (sin comprimir, igual), ni el terreno instanciado
(`drawInstanced = false`, igual), ni el lightmap en sí (exportado a PNG está
sano: se ven el terreno y las sombras de los árboles). La suavidad de las capas
(alfa de la textura, ver U6) tampoco lo explicaba sola.

**Decisión:** el terreno no se hornea (sin `ContributeGI`); se ilumina en tiempo
real y recibe el ambiente por sondas. **Revisar al subir a 6.7 LTS.** Si sigue,
la salida prevista es un shader toon de terreno propio que lea el lightmap.

### U6. El Terrain de URP lee el alfa de la textura de capa como suavidad

Una textura de capa sin canal alfa se lee con alfa 1: suavidad máxima, el suelo
es un espejo. No se nota hasta que hay algo que reflejar (al hornear se crea la
sonda de reflexión del cielo). Las superficies se hornean con alfa 0
(`TerrainSurfaceRecipe`), y la escena no usa reflejos de entorno.

### U7. Restos de trabajo dentro de un .glb

El `.glb` de Luna (y el de Artemis) traía una `Icosphere` de 2 m además del
gato: en Godot quedaba oculta o no se notaba, en Unity el modelo medía 2 m. La
lista `QUITAR` de `tools/importar_desde_godot.py` los borra al convertir. Al
importar un modelo nuevo, comprobar su altura
(`tools/unity_mcp.py` + `Renderer.bounds`).

### U8. `manage_build` y `execute_code` bloquean el puente mientras duran

Una build de Android ocupa el hilo principal del editor minutos (IL2CPP la
primera vez): cualquier otra llamada al MCP espera y puede agotar su tiempo.
Lanzar la build y consultar su estado cuando acabe; no encadenar otras órdenes.

### U9. La conversión de ejes de Blender, aplicada a Mixamo, pone al personaje de espaldas

**Síntoma:** Serena corre hacia delante mirando hacia atrás. Medido: la raíz
avanza por +Z y la punta del pie apunta a −Z.

**Causa:** `bakeAxisConversion` convierte los ejes de Blender (−Y adelante,
Z arriba) a los de Unity. Los FBX de Mixamo ya vienen en los de Unity (+Z
adelante, Y arriba): convertirlos otra vez los gira 180°.

**Arreglo:** en `ModelImportRules`, la conversión solo para lo que sale de
Blender (props y personajes convertidos de .glb), nunca para Mixamo. **Al
importar un modelo de origen nuevo, medir hacia dónde apunta el pie mientras
anda** (el `bodyRotation` del Animator no sirve para esto: en estos avatares
sale girado aunque el modelo esté bien).

### U10. Avatares Humanoid fuera de pose T: articulaciones retorcidas

**Síntoma:** en Play, rodillas juntas, pies girados y la falda pellizcada
entre los muslos. Quieta y corriendo.

**Causa:** Unity traduce cada animación de un cuerpo a otro pasando por un
espacio común que da por hecho que **cada avatar está en pose T**. Ninguno lo
estaba: el modelo de Serena (Mixamo «con piel», pasado por Blender) viene en
pose A, y los clips «sin piel» traían reposos desviados entre 60° y 87° de la
horizontal en los brazos. En Godot no se notaba porque las pistas se aplicaban
hueso a hueso, sin traducir (ver `diseno/10-ANIMATION-TREE.md` §4 del
proyecto viejo: «Mixamo exporta reposos distintos»).

**Probado y descartado:** usar el avatar de Serena para todos los clips
(`CopyFromOther`). Unity avisa «Bone length in configuration does not match
position in animation file» con 19 cm de desfase en la cadera: los clips traen
otro esqueleto.

**Arreglo:** `HumanoidTPose.cs` hace lo que el botón «Enforce T-Pose» del
configurador de avatares, en la importación y para todos: regenera la
descripción del avatar desde el archivo y pone en T **solo las rotaciones** de
brazos, antebrazos, manos, muslos y piernas.

**Ojo, segunda trampa dentro de esta:** la primera versión copiaba también
POSICIONES y el resto de nodos del esqueleto. El FBX de Serena trae el
esqueleto desplazado respecto a la malla (cadera a 7 cm, pies a −0,77 m en
reposo), y sobrescribir esas entradas le dejó una escala humana de **0,06**
(las demás, 0,94): quedaba hundida en el suelo hasta la falda. Comprobación
rápida tras tocar un avatar: `Animator.humanScale` tiene que salir cerca de la
altura de la cadera (0,9 para estas alturas).

### U11. Clips de Mixamo que hacen más de lo que dice su nombre

**Síntoma:** al aterrizar, Serena parecía saltar otra vez.

**Causa:** `land` («Falling To Landing») empieza en el aire, 0,7 m por encima
de la altura normal, y no toca suelo hasta 0,27 s. `jump_start` («Jump») es un
salto entero con su propio aterrizaje. Se midió con la altura del cuerpo
(`RootT.y`) a lo largo de cada clip.

**Arreglo:** `ModelImportRules.Ranges` recorta cada clip al tramo útil al
importar. **Antes de montar un estado con un clip nuevo de Mixamo, medir su
`RootT.y`** para ver qué hace de verdad (la herramienta está en la sesión del
2026-09-29: muestrear la curva con `AnimationUtility.GetEditorCurve`).

### U12. Dentro de una regla de importación no se pueden cargar otros assets

**Síntoma:** la tabla de escalas calculaba bien, pero los modelos se quedaban a
escala 1 y la consola decía «no está en la tabla» de modelos que sí estaban.

**Causa:** Unity 6 importa modelos en procesos paralelos, y ahí
`AssetDatabase.LoadAssetAtPath` no carga otros assets: devuelve nada, en
silencio.

**Arreglo:** la herramienta de editor escribe el valor en el importador de cada
modelo (`importer.globalScale` + `SaveAndReimport`): queda en su `.meta`. Desde la
regla, como mucho se lee el archivo del disco (`File.ReadAllText`).
**Regla:** un `AssetPostprocessor` solo decide con lo que tiene su propio archivo.

### U13. Cinemachine manda sobre la cámara, y trae 0,1 → 5.000 m

La `Camera` puede decir otra cosa: la lente de la `CinemachineCamera` la pisa en
cada fotograma. Por defecto trae recorte de 0,1 a 5.000 m (poca precisión de
profundidad, todo el mapa en cola de dibujo). Lo fija `AdaptiveLens` junto al
campo de visión.

### U14. `GetComponent` devuelve un «nulo falso» que `is { }` no detecta

En el editor, un componente que no existe devuelve un objeto que compara igual a
`null` con `==`, pero **no** con el patrón `is { } x` de C#. Usar
`TryGetComponent(out var x)`.
