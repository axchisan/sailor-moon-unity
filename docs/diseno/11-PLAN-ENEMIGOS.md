> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/11-PLAN-ENEMIGOS.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Plan de assets de enemigos

> **Actualizado el 2026-08-04.** El Peluchín ya está terminado y sirve de
> plantilla para todo lo demás.

## 1. El hueco que había

Los modelos de Peluchín y Cofrecito llevaban días generados y procesados en
`assets/models/characters/`, pero **no estaban conectados al juego**: las
escenas de enemigo seguían usando una esfera y una caja de prototipo. Solo se
veían en una escena de pruebas.

Cerrar ese hueco obligó a resolver tres cosas que ahora valen para todos los
enemigos que vengan.

## 2. Lo que hubo que resolver (y ya está hecho)

### 2.1 Animación procedural — `src/enemies/blob_animator.gd`

Los enemigos **no se riguean**. El Peluchín es una bola con patitas y el
Cofrecito un cofre: Mixamo solo riguea humanoides, y aunque pudiera, un
esqueleto para una bola es tirar el trabajo. Se ve mejor con *squash & stretch*,
que además es el recurso clásico de la animación de dibujos.

| Estado | Deformación | Medido |
|---|---|---|
| `Idle` | Respiración suave | rebote 0,035 m |
| `Approach` | Saltitos + inclinación al avanzar | rebote 0,075 m |
| `Wait` | Balanceo lateral impaciente | ±9° |
| `Telegraph` | Se agacha y vibra | escala Y **1,00 → 0,75** |
| `Attack` | Estirón hacia delante | escala Y **0,72 → 1,26** |
| `Stagger` | Vuelta de campana | giro 14 rad/s |
| `Defeated` | Lo lleva `e_defeated.gd` | — |

Cada instancia arranca con un **desfase aleatorio**, o rebotarían todas a la vez
como un coro y se notaría que es procedural.

> **Trampa:** la conexión a `state_changed` NO puede hacerse en `_ready()`. Los
> hijos se inicializan antes que el padre, así que `Enemy.state_machine` (que es
> `@onready`) todavía es `null` y la señal se pierde en silencio: el enemigo se
> queda en `Idle` para siempre. Se engancha en el primer `_process`.

### 2.2 Materiales MToon

`enemy.gd` teñía al enemigo con `albedo_color` de `StandardMaterial3D`. Con el
modelo real el material es **MToon (ShaderMaterial)** tras pasar por `ToonRoot`,
así que el tinte no hacía nada — **y se perdía el aviso rojo previo al golpe**,
que es una regla de compasión del diseño.

`set_color()` y `CombatFeel.flash()` ahora aceptan los dos tipos. Verificado: el
color oscila entre el lavanda base y el rojo de aviso.

### 2.3 Orientación

Los modelos de hi3d.ai **miran hacia +Z** y Godot usa **−Z** como frente. Sin
girarlos, el enemigo le daba la espalda al jugador al "mirarlo".

Se corrige girando el nodo `Visual` 180° en Y. Verificado midiendo
`get_forward().dot(direction_to_target())` = **1,00**.

## 3. Jerarquía de una escena de enemigo

Esta estructura es la plantilla. **Respetarla**: cada nivel tiene un motivo.

```
Peluchin (CharacterBody3D)   capa 4, máscara 5
├── CollisionShape3D
├── Model (Node3D)           ← lo ROTA face_target(). No se escala nunca
│   ├── Deform (Node3D)      ← lo deforma BlobAnimator. Aquí no hay colisión
│   │   └── Visual (ToonRoot)   girado 180° en Y
│   │       └── modelo.glb
│   └── Hitbox (Area3D)      ← fuera de Deform: el squash no debe alterarla
├── Hurtbox (Area3D)
├── Animador (BlobAnimator)  modelo_path → ../Model/Deform
└── StateMachine (7 estados)
```

**Por qué `Hitbox` cuelga de `Model` y no de `Deform`:** si estuviera dentro del
nodo que se deforma, al aplastarse el enemigo su zona de golpe cambiaría de
tamaño. El daño no debe depender de la animación.

## 4. Estado y trabajo restante

| Enemigo | Modelo | Escena | Comportamiento | Estado |
|---|---|---|---|---|
| **Peluchín** (básico) | ✅ 5.000 tris | ✅ | ✅ acercarse y golpear | **Terminado** |
| **Cofrecito** (escudo) | ✅ 5.000 tris | ✅ | ✅ escudo frontal 130° | **Terminado** |
| **Hadita** (a distancia) | ✅ 5.000 tris | ✅ | ✅ vuela, dispara y huye | **Terminado** |

### 4.1 Cofrecito — TERMINADO

| Propiedad | Valor | Por qué |
|---|---|---|
| Vida | 4 | Aguanta más que el Peluchín (3) |
| Arco de escudo | **130°** frontal | Bloquea de frente, pasa por los lados |
| Velocidad de giro | **2,2** (Peluchín: 9,0) | **Es lo que lo hace jugable** |
| Velocidad | 2,4 | Más pesado y lento |
| Ataque | `enemy_cofrecito.tres` | Aviso 0,9 s, más retroceso |

**El detalle de diseño que importa:** el enemigo gira para encararte, así que un
escudo frontal bloquearía SIEMPRE si girase rápido. Lo que lo hace jugable es su
`turn_speed` de 2,2: tarda en encararte y rodearlo funciona.

> ⚠️ **`shield_arc` y `turn_speed` van juntos.** Si alguien sube la velocidad de
> giro del Cofrecito, se vuelve invencible salvo con el especial.

**Cómo se le gana** — tres caminos, y los tres enseñan algo:
1. Rodearlo mientras gira despacio
2. Golpearle mientras está comprometido en su ataque
3. **Reventarle la guardia con el especial**, que ignora el escudo
   (`breaks_shield` en `AttackData`). Así la barra deja de ser opcional

**Verificado:** 2 golpes de frente → salud 4 → 4 (bloquea). 1 golpe por la
espalda → 4 → 2 (pasa). Especial de frente → destruido.

Al bloquear no hay hit stop: congelar el juego premiaría un golpe que no ha
servido. Solo un *clonk* metálico, chispas doradas y un destello del escudo.

### 4.2 Hadita — enemigo a distancia

Sustituye al "Globito" del diseño original. Es un **hada de espejo roto**:
encaja mejor porque son secuaces directos de la Dama del Espejo Roto, así que se
entiende de dónde salen sin explicar nada.

#### Prompt de IMAGEN (paso 1: generar la referencia)

hi3d.ai solo acepta imagen, así que primero se genera la referencia. Este prompt
está escrito para un generador de imágenes, no de 3D:

```
A cute chibi fairy character, full body, front view, standing centered,
collectible figure style.
Small girl fairy with pale lavender skin and short silver-white hair,
large mischievous eyes with white sparkle highlights, small pointed ears,
a simple lilac dress with silver trim, a cracked mirror shard set into the chest
of the dress like a brooch.
OPAQUE faceted crystal wings in pale lilac and silver, thick and solid like
carved gemstone, with a pearlescent sheen, wings clearly separated from the body.
Small mirror shards attached along the wing edges and shoulders, NOT floating
in the air.
Soft vinyl toy aesthetic, magical girl anime minion, mischievous but cute,
not scary.
Cel shaded flat colors with clean color separation, soft even studio lighting
from the front, no cast shadows.
Pure white background, product photo of a collectible figure, high resolution,
sharp focus, centered, full body visible.
```

**Negativo:**

```
transparent, translucent, see-through, glass, thin membrane wings,
floating detached pieces, particles, dark background, dramatic lighting,
cast shadows, motion blur, cropped, close-up, multiple characters, text,
watermark, photorealistic, scary, sharp teeth, weapons
```

> ⚠️ **Dos trampas, y las dos vienen del propio concepto de hada de cristal.**
>
> **1. Nada transparente.** Unas alas de cristal transparentes son lo natural de
> pedir y lo peor posible para imagen → 3D: el reconstructor no puede deducir la
> forma de algo a través de lo cual se ve el fondo, y devuelve alas con agujeros
> o directamente sin alas. Van **opacas y facetadas**, como gema tallada. El
> efecto de cristal se hace luego en Godot con el material, no con la geometría.
>
> **2. Nada de esquirlas flotando sueltas.** Lo bonito sería un halo de trozos de
> espejo alrededor. Pero la geometría desconectada del cuerpo la reconstruye
> fatal: o la funde en pegotes o la descarta. Las esquirlas van **pegadas a las
> alas y los hombros**. Si quieres el halo flotante, se hace en Godot con
> partículas o mallas hijas orbitando, y encima queda mejor porque se mueve.

#### Comprobar antes de subirlo a hi3d.ai

- [ ] Cuerpo entero dentro del encuadre
- [ ] Fondo blanco liso, sin sombra proyectada en el suelo
- [ ] **Alas separadas del cuerpo**, con hueco visible entre ellas y la espalda
- [ ] Nada transparente y nada flotando suelto
- [ ] Mínimo 1040×1040

Genera 3 o 4 variaciones y quédate con la de **silueta más legible**, no con la
más bonita: la IA de 3D reconstruye la forma, no el arte.

#### Comportamiento — TERMINADO

| Propiedad | Valor | Por qué |
|---|---|---|
| Vida | **2** | La más frágil: el precio de ser molesta desde lejos |
| Alcance | 7,0 m | Ataca sin acercarse |
| Distancia de huida | **3,2 m** | Retrocede si te pegas a ella |
| Vuela a | 1,0 m | No pisa el suelo |
| Aviso | 0,85 s | Como todos: nunca dispara sin telegrafiar |

**Tres piezas nuevas que ahora sirven para cualquier enemigo volador o a
distancia:**

1. **Vuelo** (`Enemy.flota`): ignora la gravedad y busca su altura con un muelle
   amortiguado sobre un rayo al suelo. Sin la amortiguación rebotaría como una
   pelota
2. **Proyectil** (`src/enemies/projectile.gd` + `scenes/enemies/esquirla.tscn`):
   se mueve a mano y comprueba el suelo con un rayo. Un `RigidBody3D` para esto
   sería pagar por nada
3. **Huida** (`Enemy.huir_si_esta_cerca()`): usada desde `Approach` y `Wait`

> **El proyectil va a 5 m/s a propósito.** Uno rápido en un juego para una niña
> de 8 años es imposible de esquivar y se percibe como injusto. Este se ve venir
> de lejos y se esquiva andando de lado, que es justo la lección que enseña
> este enemigo.

**Verificado:** altura estable a 1,00 m sin oscilar, 2 esquirlas lanzadas,
al acercarse a 1,2 m retrocedió hasta 6,0 m, y el impacto baja la salud del
jugador de 5 a 4.

### 4.3 Variantes por nivel — el truco que ahorra el 80%

**No se generan modelos nuevos por nivel.** Se reutilizan los tres arquetipos
cambiando `base_color` y, como mucho, añadiendo un accesorio suelto:

| Nivel | Tinte | Idea |
|---|---|---|
| 1 · Parque | Lavanda (actual) | base |
| 2 · Templo | Rojo teja | pañuelo de cuervo |
| 3 · Escuela | Azul tiza | gorro de graduación |
| 4 · Arcade | Neón magenta | ojos de píxel |
| 5 · Teatro | Dorado | antifaz |

Cambiar un color es un `@export`; generar un modelo son créditos, minutos de
Blender y peso en el APK.

## 5. Intensidad — dónde están las palancas

Tras pedir la jugadora más hostilidad, estos son los valores actuales y dónde
tocarlos:

| Qué | Antes | Ahora | Dónde |
|---|---|---|---|
| Atacando a la vez | 3 | **4** | `combat_director.gd` → `max_attackers` |
| Aviso antes de golpear | 1,2 s | **0,75 s** | `enemy_basic.tres` → `windup` |
| Velocidad de avance | 2,6 | **3,4** | `enemy.gd` → `move_speed` |
| Recuperación tras golpear | 0,7 s | **0,45 s** | `enemy_basic.tres` → `recovery` |
| Aturdimiento al ser golpeado | 0,30 s | **0,22 s** | `e_stagger.gd` → `STAGGER_TIME` |
| Distancia a la que rondan | 3,2 | **2,8** | `enemy.gd` → `wait_distance` |

**Lo que NO se tocó, y no se debe tocar:** el aviso rojo antes de cada golpe.
Puede acortarse, pero jamás quitarse. Es lo que separa "difícil y divertido" de
"injusto", y a los 8 años esa frontera se cruza muy rápido.

Si en el móvil resulta demasiado, la palanca más suave es subir `windup` a 0,9;
la más agresiva a la baja es bajar `max_attackers` a 3.

## 6. Orden de trabajo sugerido

1. ~~Cofrecito~~ ✅ **Terminado y mezclado en las oleadas**
2. **Probar en el móvil** la hostilidad y el escudo antes de añadir más
3. **Hadita** — necesita modelo, proyectil y un estado nuevo. Es el más caro
4. **Variantes de color** cuando existan los niveles 2 y 3

### Oleadas mixtas

`Arena` ahora acepta `enemy_scenes_extra` y los mezcla con el enemigo básico:

| Ajuste | Valor | Para qué |
|---|---|---|
| `extra_ratio` | 0.34 | Un tercio de la oleada son extra |
| `extra_from_wave` | 1 | La **primera** oleada es solo Peluchines |

Ese `extra_from_wave` es deliberado: primero se aprende a pegar, y solo cuando
eso ya funciona aparece el enemigo que obliga a rodear. Una mecánica cada vez.
