> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/12-REPARTO.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Reparto de personajes

> **2026-08-07.** Reparto **completo**: once personajes procesados, rigueados en
> Mixamo y verificados con `tools/validar_rigueados.gd`.

## 1. Estado del reparto

| Personaje | Altura | Triángulos | Modelo | Rig | En juego |
|---|---|---|---|---|---|
| **Chibi Moon** | 1,10 m | 25.000 | ✅ | ✅ | ⬜ |
| **Sailor Saturno** | 1,35 m | 25.000 | ✅ | ✅ | ⬜ |
| **Serena** (Sailor Moon) | 1,55 m | 25.000 | ✅ | ✅ + pelo | ✅ **Jugable** |
| **Sailor Mercury** | 1,58 m | 25.000 | ✅ | ✅ | ⬜ |
| **Sailor Mars** | 1,60 m | 25.000 | ✅ | ✅ | ⬜ |
| **Sailor Venus** | 1,62 m | 25.000 | ✅ ⚠️ | ✅ | ⬜ |
| **Sailor Neptuno** | 1,62 m | 25.000 | ✅ | ✅ | ⬜ |
| **Sailor Jupiter** | 1,68 m | 25.000 | ✅ ⚠️ | ✅ | ⬜ |
| **Sailor Uranus** | 1,70 m | 25.000 | ✅ ⚠️ | ✅ + botas despegadas | ⬜ |
| **Sailor Pluto** | 1,75 m | 25.000 | ✅ | ✅ + botas despegadas | ⬜ |
| **Tuxedo Mask** | 1,80 m | 25.000 | ✅ | ✅ + bastón fijado | ⬜ |

⚠️ = llevaban geometría de más pegada, ya separada (§2).

### Comprobación automática del rigueo

```
Godot --headless --path . --script tools/validar_rigueados.gd
```

Recorre todos los `*_rigged.fbx` y avisa de las tres cosas que rompen un
personaje: que no tenga esqueleto, que le falte algún hueso del núcleo
humanoide, o que `Hips` venga girado (síntoma de haberlo bajado de Mixamo en
glTF en vez de en FBX — el personaje sale tumbado).

**El número de dedos NO importa.** Mixamo deja elegir cuántas falanges genera y
en el reparto hay de todo (40, 16 y 8 huesos de dedos). Godot ignora sin
quejarse las pistas de animación de un hueso que no existe.

Las alturas están puestas a escala real y ordenadas a propósito: de 1,10 m
(Chibi Moon, una niña) a 1,80 m (Tuxedo Mask). Puestos en fila se lee como un
reparto de verdad, y en combate la diferencia de tamaño da información al
instante sobre a quién tienes delante.

Procesados de **~2.000.000 de triángulos y texturas 4K** a 25.000 y 2048, con
`tools/blender_preparar_modelo.py`.

## 2. Objetos que venían pegados al personaje

El generador 3D añadió decorado que no se pidió y lo dejó **soldado a la misma
malla** del personaje. No se ve como un objeto aparte: es la misma malla.

| Personaje | Qué traía | Coste |
|---|---|---|
| **Sailor Venus** | Un coral marino a sus pies | 4.702 tris (≈19 % de su presupuesto) |
| **Sailor Jupiter** | Un arbusto tipo bonsái a sus pies | 9.752 tris (≈39 % de su presupuesto) |
| **Sailor Uranus** | Una espada en la mano | 3.000 tris |

**No se borraron: se separaron**, y viven en `assets/models/props/` como
`coral_marino.glb`, `arbusto_bonsai.glb` y `espada_uranus.glb`. Sirven de
decorado para los escenarios ([`07-ESCENARIOS.md`](07-ESCENARIOS.md)) — el
bonsái encaja en un jardín y el coral en un nivel marino. La espada tiene el
origen puesto en el puño, así que se puede colgar de un `BoneAttachment3D` si
algún día se le quiere devolver.

Al quitárselos, Venus y Jupiter **recuperaron su presupuesto entero**: los
25.000 triángulos son ahora todos personaje. Antes, 4 de cada 10 triángulos de
Jupiter eran un arbusto.

### Cómo se hizo (por si aparece otro)

1. Se agrupan las islas de malla del **GLB ya procesado** con union-find sobre
   sus cajas de contorno. Aparecen los grupos separados del cuerpo.
2. Se exporta cada grupo suelto como prop y **se anota su caja en metros**.
3. Se **reprocesa el personaje desde el original**, borrando los vértices dentro
   de esa caja *antes* de decimar. Así la decimación reparte los 25.000
   triángulos solo entre el personaje.

> **Trampa:** el agrupado hay que hacerlo sobre el **GLB procesado**, no sobre
> el original. Exportar a glTF duplica vértices en las costuras de UV, lo que
> parte la malla en islas separables (199 en el procesado). El original está
> genuinamente más conectado: solo da 5 islas, y el coral toca la mano de Venus
> a 5 cm, así que a cualquier umbral se fusiona con ella.

## 3. Buena noticia: vienen en T-pose

A diferencia de Serena —que llegó con los brazos casi pegados y hubo que
abrírselos a mano en Blender— **el resto viene ya en T-pose**, que es
exactamente lo que quiere el auto-rigger de Mixamo. No hay que tocarles la pose.

**Excepción: Sailor Uranus venía girada 45°.** En T-pose, pero de tres cuartos
en vez de mirando al frente, y así Mixamo no consigue riguearla. Se detecta sin
mirarla: si la envergadura y la profundidad se parecen (1,10 y 1,16 en su caso,
frente a 1,45 y 0,45 de las demás), es que está girada. El ángulo exacto sale
del eje principal de la nube de vértices a la altura de los hombros —para ella,
44,8°—, y se corrige rotando la malla ese ángulo en Z.

> **Trampa:** el ángulo dice cuánto girar, pero **no si el resultado queda de
> frente o de espaldas**. Eso hay que mirarlo con un render; los intentos de
> deducirlo por la punta de los pies dan resultados equivocados.

## 3 bis. Objetos en la mano y rigueo

Un objeto sujeto en la mano y soldado a la malla del personaje **rompe el
rigueo**, y de dos formas distintas:

**1. Mixamo no encuentra las articulaciones.** Le pasó a Sailor Pluto con su
báculo: hubo que regenerarla sin él. La espada de Uranus se le quitó en Blender
por el mismo motivo.

**2. Si sí riguea, el objeto queda repartido entre varios huesos.** Es lo que
pasó con el bastón de Tuxedo Mask: Mixamo pesó su punta de abajo al **índice** y
la de arriba al **pulgar**. En cuanto la animación mueve los dedos, el bastón se
dobla y se encoge como si fuera de goma.

El arreglo es asignar **todo el objeto a un solo hueso, con peso 1,0**:
`mixamorig:LeftHand` para el bastón, `mixamorig:RightHand` para la rosa. Así se
comporta como lo que es: un objeto rígido sujeto por la mano. Ya está aplicado
sobre `tuxedo_mask_rigged.fbx`.

> **Trampa al seleccionar el objeto:** el primer filtro fue «x más allá del
> brazo», y se llevó por delante **la parte baja de la capa**, que se ensancha
> hasta esa misma X pero mucho más abajo. Hay que acotar también en altura
> (`z > 0,95` para el bastón). Si la capa acaba pesada a la mano, el personaje
> arrastra media capa cada vez que mueve el brazo.

## 4. Lo que hay que hacer con cada uno

### Paso 1 — Riguear en Mixamo ✅ hecho

Los FBX de partida están en `Models/mixamo/` por si hay que rehacer alguno. Se
sube cada uno, se marcan barbilla, muñecas, codos, rodillas e ingle, y se
descarga **con skin**. Nada más: **no hace falta bajar ni una animación**.

Lo descargado se guarda como `assets/models/characters/<nombre>_rigged.fbx` y se
pasa por `tools/validar_rigueados.gd`.

### Paso 1 bis — Las botas se pegan entre sí

Le pasó a **Uranus y a Pluto**: al separarse las piernas aparece una membrana
que une las dos botas y se estira como un chicle.

Son **dos problemas superpuestos**, y hay que arreglarlos en este orden:

**1. Pesos cruzados.** Mixamo pesa la cara interna de cada bota con la pierna de
enfrente, porque están a un par de centímetros. Se ve en los números: vértices
con 0,64 en una pierna y 0,36 en la otra. La solución es quitarle a cada vértice
el peso de la pierna contraria y renormalizar lo que queda.

**2. Una lámina de malla que sí une las dos botas.** Viene del generador 3D.

> **Por qué el orden importa:** con los pesos sucios la lámina es imposible de
> identificar — todo vértice de la zona parece pertenecer a las dos piernas.
> Una vez limpios los pesos, cada vértice pertenece a una sola, y **la lámina es
> exactamente el conjunto de caras que tocan las dos**. Se borra y se cierra el
> agujero. En Uranus eran 70 caras, en Pluto 32.

Hay que acotar el arreglo a `z < 0,75`: por encima está la entrepierna, donde la
mezcla entre las dos piernas es legítima y quitarla rompería la pelvis.

> **Lo que NO hay que perseguir:** el borde superior de las botas de Uranus tiene
> picos y muescas irregulares. **Eso ya viene en el modelo generado** — se
> comprueba abriendo el `.glb` sin riguear. No es del rigueo ni del recorte, y
> tratar de limarlo solo erosiona el borde de la bota.

El archivo descargado se guarda como
`assets/models/characters/<nombre>_rigged.fbx`, igual que Serena.

> **Trampa ya pisada:** Mixamo ofrece exportar en glTF. **No.** El glTF sale con
> 90° de más en `Hips` y el personaje aparece tumbado en Godot. Siempre **FBX**.

### Paso 2 — Reutilizar las animaciones que ya existen

Aquí está el pago de una decisión que se tomó al principio del proyecto:

> *"Todas comparten el mismo esqueleto humanoide y las mismas animaciones. Se
> diferencian por VFX, proyectiles, alcance y ritmo — no por animación nueva."*

Mixamo genera **el mismo esqueleto `mixamorig:*` para todos**. La
`AnimationLibrary` que ya está montada (`serena_animaciones.res`, 15
animaciones) **funciona tal cual sobre cualquiera de ellos**. Cero descargas
nuevas, cero trabajo de animación.

Un personaje nuevo se monta con la misma jerarquía de `player.tscn`:
`CharacterAnimator → HairPhysics → ToonRoot → modelo.fbx`.

### Paso 3 — Física de pelo y capa

Como con Serena, Mixamo no riguea nada que no sea el esqueleto humanoide. Lo que
cuelga se queda rígido y se ve mal:

| Personaje | Qué necesita cadenas de spring bones |
|---|---|
| **Tuxedo Mask** | **La capa.** Es lo más visible del personaje y rígida cantaría muchísimo |
| **Sailor Venus** | Melena larguísima, hasta las rodillas: es la que más lo nota |
| **Sailor Mars** | Melena larga hasta la cintura |
| **Sailor Pluto** | Melena larga |
| **Chibi Moon** | Las dos coletas |
| **Sailor Jupiter** | La coleta alta |
| **Sailor Neptuno** | Melena media (opcional) |
| Saturno, Uranus, Mercury | Pelo corto: no hace falta |

El procedimiento está resuelto y documentado en
[`09-FISICA-PELO.md`](09-FISICA-PELO.md): se trazan las mechas, se les crea una
cadena de huesos colgando de `Head`, se repintan los pesos con transición suave
y `SpringBoneChain` las mueve en Godot.

**La capa de Tuxedo Mask es un caso nuevo:** cuelga de la espalda (`Spine2`), no
de la cabeza, y es una superficie ancha en vez de una mecha. Necesita **3–4
cadenas en paralelo** en lugar de una, y `max_angle` bajo para que no se levante
como un ala.

### Paso 4 — Objetos en la mano

Ya resuelto para los tres casos: ver §3 bis. Uranus va sin espada, Pluto se
regeneró sin báculo, y el bastón y la rosa de Tuxedo Mask están fijados a un
solo hueso cada uno.

## 5. Presupuesto: cuidado con cuántos hay a la vez

Cada personaje son **25.000 triángulos, y el contorno del cel shading los
duplica a 50.000** de coste de dibujado. El presupuesto del proyecto es de
150.000 triángulos en pantalla ([`01-ARQUITECTURA.md`](01-ARQUITECTURA.md) §8).

| Escena | Triángulos aprox. |
|---|---|
| 1 jugadora + 8 enemigos | 50.000 + 80.000 = **130.000** ✅ |
| 2 personajes jugables + 6 enemigos | 100.000 + 60.000 = **160.000** ⚠️ |
| Las 11 del reparto a la vez | **550.000** ❌ |

**Conclusión:** en combate hay **una sola** jugable en pantalla. Para escenas de
grupo (el final del juego, un menú de selección), o se bajan a 10.000 tris con
una segunda pasada de decimación, o se les quita el contorno.

## 6. Cómo se ordenan en la historia

Ya está el reparto completo del guion original (Moon, Mercury, Mars, Jupiter,
Venus) **más** el sistema solar exterior (Uranus, Neptuno, Pluto, Saturno),
Chibi Moon y Tuxedo Mask.

La decisión tomada es **no meterlos todos de golpe**: cada escenario presenta a
uno o dos, con su momento de encuentro y su motivo para unirse. Se van
encontrando y desbloqueando conforme avanza la historia.

El reparto por nivel se define en [`07-ESCENARIOS.md`](07-ESCENARIOS.md) y el
guion en [`00-GDD.md`](00-GDD.md). Chibi Moon sigue reservada como el personaje
sorpresa del final.
