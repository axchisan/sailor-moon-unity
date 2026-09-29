> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/08-ANIMACIONES-MIXAMO.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Animaciones de Mixamo — especificación completa

> El rigueo de Serena ya funcionó. Este documento dice **exactamente** qué
> descargar, con qué ajustes, y cómo se conecta cada animación al código que ya
> existe.

## 1. Ajustes de descarga (los mismos para todas)

En el diálogo **Download Settings**:

| Campo | Valor | Por qué |
|---|---|---|
| **Format** | `FBX Binary (.fbx)` | Es el que Godot y Blender leen sin problemas. **No** uses *FBX for Unity* |
| **Skin** | Ver abajo ⬇️ | |
| **Frames per Second** | `30` | Suficiente para móvil. 60 duplica el tamaño sin ganancia visible |
| **Keyframe Reduction** | `none` | La reducción destroza los golpes rápidos, que duran 3 frames |

### La regla del Skin

- **La PRIMERA descarga: `With Skin`.** Trae la malla con el esqueleto. Es tu
  modelo rigueado definitivo. Descarga con ella la animación `idle`.
- **Todas las demás: `Without Skin`.** Traen solo el movimiento del esqueleto.
  Pesan una fracción y evitan tener 15 copias de la malla.

Si ya bajaste alguna *With Skin* por error, no pasa nada: se ignora la malla
duplicada al importar.

## 2. Parámetros por animación

Los deslizadores del panel derecho cambian según la animación. Los que importan:

- **In Place** — quita el desplazamiento. **Obligatorio en locomoción**, porque
  quien mueve al personaje es `move_and_slide()` en `player.gd`. Sin esto, el
  personaje se desplaza el doble.
- **Overdrive** — velocidad. Se usa para acercar la duración a la del código.
- **Trim** — recorta el principio y el final.
- **Character Arm-Space** — separación de los brazos. **Ojo con Serena:** tiene
  mangas abullonadas y coletas enormes; si los brazos atraviesan el pelo, sube
  este valor.

### Tabla completa

| # | Nombre a usar | Buscar en Mixamo | In Place | Overdrive | Notas |
|---|---|---|---|---|---|
| 1 | `idle` | *Breathing Idle* | — | 0 | Bucle. Descárgala **With Skin** |
| 2 | `walk` | *Walking* | ✅ **Sí** | 0 | Bucle |
| 3 | `run` | *Running* | ✅ **Sí** | 0 | Bucle |
| 4 | `jump_start` | *Jumping Up* | ✅ Sí | +20 | Solo el impulso. Recorta el aterrizaje con Trim |
| 5 | `jump_loop` | *Falling Idle* | ✅ Sí | 0 | Bucle. Sirve para saltar y para caerse |
| 6 | `land` | *Hard Landing* / *Falling To Landing* | ✅ Sí | +30 | Corta y seca |
| 7 | `attack_1` | *Punching* / *Cross Punch* | ✅ Sí | **+60** | Muy rápida (ver §3) |
| 8 | `attack_2` | *Hook Punch* | ✅ Sí | **+60** | Que se note distinta de la 1 |
| 9 | `attack_3` | *Spin Kick* / *Roundhouse Kick* | ✅ Sí | **+35** | El finisher. Más lucida |
| 10 | `special` | *Standing 2H Magic Attack 01* | ✅ Sí | 0 | Pose de invocación |
| 11 | `hurt` | *Standing React Small From Front* | ✅ Sí | +40 | Muy corta |
| 12 | `dizzy` | *Stunned* / *Dizzy* | ✅ Sí | 0 | Bucle mientras se recupera |
| 13 | `victory` | *Victory* / *Cheering* | ✅ Sí | 0 | Fin de arena |
| 14 | `transform_pose` | *Praying* / *Standing Idle 01 Looking* | ✅ Sí | −20 | Para la transformación |
| 15 | `talk_idle` | *Talking* | ✅ Sí | 0 | Bucle de diálogos |

> **Regla general del In Place:** actívalo **siempre que exista**. La única
> excepción sería una cinemática donde quieras que el desplazamiento venga de la
> animación, y ahora mismo no tenemos ninguna.

## 3. Duraciones objetivo — esto es lo que más importa

El combate ya está ajustado y **se siente bien**. Las animaciones se adaptan al
código, no al revés. Estas duraciones salen de `assets/data/attacks/*.tres`:

| Animación | Duración objetivo | De dónde sale |
|---|---|---|
| `attack_1` | **0,29 s** | windup 0,07 + active 0,09 + recovery 0,13 |
| `attack_2` | **0,29 s** | 0,06 + 0,09 + 0,14 |
| `attack_3` | **0,53 s** | 0,13 + 0,14 + 0,26 |
| `special` | **1,30 s** | 0,45 + 0,30 + 0,55 |
| `hurt` | **0,35 s** | `STUN_TIME` en `hurt.gd` |
| `dizzy` | **1,60 s** | `DIZZY_TIME` en `dizzy.gd` |

Un puñetazo de Mixamo dura ~1 s y el nuestro 0,29 s. **No intentes clavarlo con
el Overdrive.** Acércalo con el deslizador y ajusta el resto en Godot con
`speed_scale` del `AnimationPlayer`:

```gdscript
# Encaja la animación en el tiempo que dicta el AttackData
var duracion_animacion := anim_player.get_animation("attack_1").length
anim_player.speed_scale = duracion_animacion / attack.total_time()
anim_player.play("attack_1")
```

Así el golpe visual y la hitbox van sincronizados por construcción, y si mañana
ajustas el `.tres` para que el combate se sienta mejor, la animación se adapta
sola.

## 4. Cómo se conecta al código existente

Cada estado en `src/player/states/` reproduce su animación al entrar:

| Estado | Animación |
|---|---|
| `Idle` | `idle` |
| `Move` | `walk` ↔ `run` mezcladas por `get_move_strength()` |
| `Jump` | `jump_start` → `jump_loop` |
| `Fall` | `jump_loop` |
| `Attack` | `attack_1` / `attack_2` / `attack_3` según `combo_index` |
| `Special` | `special` |
| `Hurt` | `hurt` |
| `Dizzy` | `dizzy` |

Cuando tengas los FBX descargados, yo me encargo de:

1. Importarlos y **retargetearlos** al esqueleto (ya validamos que el mapeo de
   huesos humanoide de Godot funciona, ver [`04-VALIDACION-VRM.md`](04-VALIDACION-VRM.md))
2. Meterlos en una `AnimationLibrary` única
3. Montar el `AnimationTree` con la mezcla walk↔run
4. Añadir la llamada a `play()` en el `enter()` de cada estado
5. **Mover la activación de la hitbox a la animación**, con *call method tracks*.
   Es el paso que quedó pendiente de la Fase 1: hoy la hitbox la enciende un
   temporizador del estado; con la animación real, se enciende en el frame exacto
   del impacto y el golpe se siente mucho mejor

## 5. Dónde dejar los archivos

```
assets/animations/serena/
├── idle.fbx            ← la única With Skin
├── walk.fbx
├── run.fbx
├── jump_start.fbx
├── jump_loop.fbx
├── land.fbx
├── attack_1.fbx
├── attack_2.fbx
├── attack_3.fbx
├── special.fbx
├── hurt.fbx
├── dizzy.fbx
├── victory.fbx
├── transform_pose.fbx
└── talk_idle.fbx
```

**Renombra los archivos** al bajarlos: Mixamo los llama `Breathing Idle.fbx`,
`mixamo.com.fbx` o cosas peores, y con 15 archivos te pierdes.

## 6. Si algo sale mal

| Síntoma | Causa probable | Solución |
|---|---|---|
| El personaje se desplaza el doble | Falta **In Place** | Vuelve a descargar con In Place activado |
| Los brazos atraviesan las coletas | Mangas abullonadas + pelo | Sube **Character Arm-Space** |
| El golpe se ve entrecortado | **Keyframe Reduction** activo | Descarga de nuevo con `none` |
| La animación va muy lenta o rápida | Normal | Se corrige con `speed_scale` (§3) |
| El modelo aparece gigante o diminuto | Escala del FBX | Se arregla al importar en Godot |

## 7. Prioridad si quieres ir probando

No hacen falta las 15 para ver resultados. Con estas **seis** ya se puede
sustituir la cápsula y jugar:

`idle` · `walk` · `run` · `jump_loop` · `attack_1` · `hurt`

Bájalas primero y avísame: monto el `AnimationTree` con esas y seguimos
añadiendo el resto sobre la marcha.
