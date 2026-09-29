> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/05-COMBATE.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Sistema de combate — guía de ajuste

> Dónde tocar cada cosa cuando el combate no se sienta bien.
> Escena de pruebas: `scenes/prototypes/test_combat.tscn`

## Mapa rápido: "quiero cambiar X"

| Quiero... | Archivo |
|---|---|
| Que los golpes se sientan más contundentes | `src/autoload/combat_feel.gd` |
| Cambiar daño, alcance o tiempos de un golpe | `assets/data/attacks/*.tres` |
| Que el combo sea más o menos permisivo | `combo_window` en `src/player/player.gd` |
| Que apunte mejor o peor a los enemigos | `get_attack_target()` en `player.gd` |
| Cuántos enemigos pueden atacar a la vez | `max_attackers` en `src/autoload/combat_director.gd` |
| Cuánto avisa un enemigo antes de pegar | `windup` en `assets/data/attacks/enemy_basic.tres` |
| Que al Cofrecito se le pueda rodear o no | `shield_arc` y `turn_speed` en `scenes/enemies/cofrecito.tscn` |
| Cuánto dura el hueco tras bloquear | `ATURDIMIENTO_BLOQUEO` en `src/enemies/enemy.gd` |
| Oleadas y tamaño de la arena | exports de `src/level/arena.gd` |
| Tamaño y posición de los botones táctiles | `scenes/ui/game_ui.tscn` |

## Los cuatro valores que más cambian la sensación

Si solo vas a tocar cuatro números, que sean estos:

1. **`hit_stop`** (en cada `.tres` de ataque, 0,045–0,10 s). Es el congelado al conectar.
   Subirlo hace los golpes más pesados; pasarse hace el combate lento y pastoso.
2. **`knockback`** (3,0 en el primer golpe, 9,0 en la patada giratoria). El contraste
   entre golpes flojos y el finisher es lo que hace el combo satisfactorio.
3. **`combo_window`** (0,8 s). Deliberadamente enorme. Bajarlo por debajo de 0,5
   rompe el pilar de "machacar siempre funciona".
4. **`STAGGER_TIME`** (0,30 s en `e_stagger.gd`). Es la ventana en la que el enemigo
   no puede contraatacar. Bajarlo vuelve el combate injusto muy rápido.

Para comparar con y sin jugo: `CombatFeel.set_juice_enabled(false)`. Jugar un minuto
con cada uno es la mejor forma de ver cuánto está aportando realmente cada efecto.

## Verificaciones automáticas hechas (2026-08-01)

Ejecutadas contra el juego corriendo, no sobre el papel:

| Prueba | Resultado |
|---|---|
| 8 enemigos rodeando al jugador → máximo simultáneo atacando | **3 / 3** ✅ |
| Combo completo: índices y daño | 1 → 2 → 3, daño **1 / 1 / 2** ✅ |
| Purificación al agotar la salud | Enemigo pasa a grupo `friends`, +3 chispas ✅ |
| Especial con 5 enemigos alrededor | Los 5 purificados de un golpe ✅ |
| Invulnerabilidad durante el especial | Activa ✅ |
| `Engine.time_scale` tras el hit stop | Vuelve a **1.0** ✅ |
| Trigger de arena al entrar caminando | Barrera activa, oleada 1 lanzada ✅ |
| Rendimiento con 9 enemigos + partículas | **60 fps** en Mobile/Metal ✅ |

## Tres bugs encontrados y corregidos

### 1. La cámara en tercera persona nunca funcionó

`CameraRig._update_shake()` hacía `camera.position = Vector3.ZERO` en cada
`_process`. Pero **`SpringArm3D` coloca a sus hijos escribiéndoles la `position`**
en el frame de física. El `_process` la pisaba después, así que la cámara se
quedaba clavada en el origen del brazo: dentro del personaje.

Lo insidioso: al consultar `camera.position` desde fuera daba el valor correcto
(5.0), porque la lectura ocurría antes de que el `_process` del rig lo pisara.
Solo se veía en lo renderizado.

**Corregido** usando `h_offset` / `v_offset` de `Camera3D` para la sacudida.
Desplazan la vista sin tocar la transformación del nodo.

> Regla general: **nunca escribas `position` en un hijo de `SpringArm3D`.**

### 2. `monitorable` dentro de señales de física

`Function blocked during in/out signal`. Ocurría al instanciar enemigos desde
`body_entered` y al purificar desde `area_entered`. Corregido con `set_deferred`
en `hitbox.gd`, `hurtbox.gd` y `Enemy.purify()`.

### 3. El especial se recargaba a sí mismo

Los impactos del propio especial cargaban la barra: golpear a 5 enemigos devolvía
el 41% de la carga. Siendo además invulnerable mientras dura, era encadenable casi
sin límite y trivializaba el combate. Ahora solo carga el combo normal.

## Lo que queda de la Fase 1

- **Globito** (enemigo a distancia) y **Cofrecito** (con escudo). El `Peluchín`
  sirve de plantilla: son el mismo esqueleto de estados con otro comportamiento.
- Prueba en el móvil **con los dedos**. El joystick flotante y el tamaño de los
  botones solo se pueden juzgar en el dispositivo real.
- **El ajuste del *feel*.** Esto no lo puede cerrar ningún test automático: hay que
  machacar el botón cinco minutos y decidir si apetece seguir. Es el criterio de
  salida de la fase y es tuyo.
