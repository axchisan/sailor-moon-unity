# Escala del mundo — el estándar

> **Regla de oro: 1 unidad de Unity = 1 metro, y todo mide lo que mediría de
> verdad.** Si algo parece pequeño o grande, se mide contra esta tabla antes de
> tocarlo. Escrito el 2026-09-30 tras la auditoría que encontró el parque «de
> juguete»: los cerezos medían 3,7 m, la cámara era un gran angular de 99° en
> el teléfono y Serena patinaba un 60 % más rápido que sus pasos.

## 1. Qué se auditó y qué salió

| Qué | Antes | Real | Ahora |
|---|---|---|---|
| Cerezo de parque | 3,3–4,8 m | 6–9 m | **7 m** (±15 %) |
| Farola | 2,1 m | 3,5–4 m | **3,6 m** |
| Estrella de Sueño | 0,9 m | objeto que se coge con las manos | **0,5 m** |
| Torii | 5,4 m | 6–9 m (santuario grande) | **7 m** |
| Hierba alta | 0,55 m | 0,3–0,4 m | **0,35 m** |
| Tocón | 0,69 m | 0,3–0,6 m | **0,5 m** |
| Campo de visión en el teléfono (20:9) | **99°** horizontales | 70–80° en juegos de tercera persona | **75°** |
| Planos de recorte de la cámara | 0,1 → 5.000 m | — | **0,2 → 400 m** |
| Andar | 2,4 m/s | 1,5 m/s (la animación) | **1,5 m/s** |
| Correr | 6,5 m/s | 3,9 m/s (la animación) | **4,8 m/s**, con la animación acelerada en proporción |
| Personajes | — | — | **Ya estaban bien** (ver §2) |

Antes y después, a la resolución del teléfono:

| Antes | Después |
|---|---|
| ![antes](img/escala/antes_arboles_telefono.png) | ![después](img/escala/despues_arboles_telefono.png) |

## 2. Personajes

Estatura medida **de la coronilla (hueso `HeadTop_End`) a la planta del
pie**, no por la caja (que incluye moños, sombreros y báculos):

| Personaje | Estatura | | Personaje | Estatura |
|---|---|---|---|---|
| Chibi Moon | 1,03–1,10 m | | Neptune | 1,62 m |
| Saturn | 1,36 m | | Uranus | 1,69 m |
| **Serena** | **1,55 m** | | Jupiter | 1,68 m |
| Mercury | 1,59 m | | Pluto | 1,72 m |
| Mars | 1,58 m | | Tuxedo Mask | 1,78 m |
| Venus | ~1,62 m | | Luna / Artemis | 0,57 m (con la cola levantada) |

Proporción anime: la cadera cae al 55–60 % de la estatura (piernas largas). Es
del estilo, no un error.

**Serena y Venus** tienen el hueso de la coronilla mal colocado (sus FBX salen
de Blender): su estatura se mide por la caja menos el peinado.

## 3. Tabla de alturas del decorado

La fuente de verdad es **`Assets/_Project/Settings/WorldScale.asset`**: cada
modelo con su altura real y **de dónde sale la medida**. Referencias de uso:

| Categoría | Medidas reales |
|---|---|
| Árboles de parque | Cerezo 6–9 m · arce japonés 5–8 m · pino de jardín 4–6 m |
| Arbustos y setos | Arbusto 0,8–1,2 m · seto bajo 0,6–0,9 m · seto alto 1,5–2 m |
| Mobiliario | Banco: asiento 0,45 m, respaldo 0,85 m, 1,6 m de largo · papelera 0,85 m · farola 3,5–4 m · fuente 1,2–1,6 m |
| Parque infantil | Columpio 2,2–2,5 m · tobogán 2,3 m |
| Arquitectura japonesa | Torii 6–9 m · farol de piedra (tōrō) 1,8–2,5 m · pagoda de 5 pisos 30–50 m |
| Calle | Puerta 2,1 m · planta de una casa 2,8–3 m · edificio de 3 plantas ~10 m · acera 2–3 m de ancho · calle de barrio 6–8 m |
| Interiores | Techo 2,4–2,7 m · mesa 0,75 m · silla 0,45 m (asiento) · cama 0,5 m · encimera 0,9 m |
| Juego | Estrella de Sueño 0,5 m · cofre de Chispas 0,6 m · Espejito 1,5–2 m |

## 4. Cámara

| Ajuste | Valor | Por qué |
|---|---|---|
| Campo horizontal | **75°** (`AdaptiveLens`) | Se fija el horizontal y el vertical sale de la pantalla: en un teléfono alargado ya no se abre un gran angular |
| Límites del vertical | 38°–52° | Ni túnel en 21:9 ni ojo de pez en 4:3 |
| Distancia | **5 m** (rueda: 3–9 m) | Serena ocupa ~45 % del alto de la pantalla |
| Inclinación | **14°** | Se ve el suelo hasta el horizonte |
| Objetivo | **1,35 m** (el pecho) | Con el objetivo en la cintura el mundo se ve desde abajo y encoge |
| Recorte | 0,2 → 400 m | Más precisión de profundidad; lo lejano lo tapa la niebla |

> **Cinemachine pisa la cámara.** Los planos de recorte y el campo de visión de
> la `Camera` no sirven: manda la lente de la `CinemachineCamera`, que por
> defecto trae 0,1 → 5.000 m.

## 5. Cómo entra un modelo nuevo (obligatorio)

1. Importarlo (`tools/importar_desde_godot.py` o a su carpeta de `Art/`).
2. **Añadir su fila** a `Settings/WorldScale.asset`: altura real y referencia.
   Sin fila, la consola avisa en cada importación.
3. `Sailor Moon ▸ Importación ▸ Normalizar escalas`: mide, calcula y reimporta.
   La escala queda escrita en su `.meta`.
4. `Sailor Moon ▸ Sandbox ▸ Sala de escala` y **Fotos de la sala de escala**:
   mirar la foto junto a Serena y la regla de metros. **Ningún modelo entra en
   un escenario sin pasar por aquí.**
5. Al colocarlo en un escenario, variación natural de ±10–15 % como mucho. **Nunca
   escalar a mano para «que se vea mejor»**: si se ve mal, está mal la tabla.

Para personajes: comprobar la estatura por `HeadTop_End` y que
`Animator.humanScale` ronde el 57 % de la estatura.

## 6. Movimiento

| Qué | Valor | Criterio |
|---|---|---|
| Andar | 1,5 m/s | Velocidad a la que la animación de andar no patina (medida) |
| Correr | 4,8 m/s | Juego ágil; la animación (3,9 m/s) se acelera ×1,23 con `LocomotionRate` |
| Salto | 1,6 m | **De juego, no realista** (una persona salta 0,5 m): las Sailor tienen poderes. Se revisa jugando en la Fase 1 |
| Cápsula | 1,55 m × 0,28 m | La estatura de Serena |

**Cómo medir la velocidad natural de un clip nuevo:** muestrear el clip y
calcular la velocidad del pie mientras está apoyado (la herramienta está en la
sesión del 2026-09-30; convertirla en menú de editor cuando haga falta otra vez).
El parámetro `Speed` del Animator va en **m/s**, y los umbrales de cada mezcla son
esas velocidades medidas.

## 7. El terreno

| Qué | Valor |
|---|---|
| Resolución | 0,94 m por celda (257 × 257 en 240 m) |
| Camino | 4,5–6 m de ancho (un paseo de parque real: 3–6 m) |
| Colinas del borde | 12–34 m a 82–116 m del centro |
| Capas | La mancha de hierba se repite cada 13,6 m (manchas de 3,4 m) |
