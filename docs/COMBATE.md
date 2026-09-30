# Combate — cómo funciona y dónde se ajusta

> Escena de pruebas: `Scenes/Sandbox/CombatSandbox.unity` (Serena, cinco
> muñecos y una roca). Se rehace con `Sailor Moon ▸ Sandbox ▸ Crear escena de
> combate`. En el teléfono es la primera escena del APK de desarrollo.

## Quiero cambiar…

| Quiero… | Dónde |
|---|---|
| Daño, empuje, avance o sensación de un golpe | `Settings/Combat/Serena_*.asset` (Inspector) |
| Que un golpe salga antes o después | `targetWindup` del ataque y **Medir impactos** |
| Que el combo sea más o menos permisivo | `_comboWindow` en `PlayerCombat` (prefab de Serena) |
| Hasta dónde se gira sola y salta hacia el enemigo | `_targetRange` y `_magnetMaxSpeed` en `PlayerCombat` |
| Cuántos golpes carga el especial y su radio | `_specialCost` y `_specialRadius` en `PlayerCombat` |
| Corazones y segundos de invulnerabilidad | `Health` del prefab de Serena |
| Cuánto dura herida o mareada | `_hurtTime` y `_dizzyTime` en `PlayerCombat` |
| Parada de impacto, sacudida, chispas, sonidos | `CombatFeel` («Sensación de combate» en la escena) y `Prefabs/Gameplay/HitSparks.prefab` |
| Qué animación suena en cada golpe | `animatorState` del ataque y los estados de `SailorLocomotion.controller` |

## Cómo está hecho

```
InputReader ─▶ PlayerCombat ─▶ PlayerMotor (bloquear, avance, empujón, girar)
                    │   └──▶ Animator (CrossFade a Attack1/2/3, Special, Hurt, Dizzy)
                    ├──▶ CombatQuery (a quién apunto, a quién le he dado)
                    ├──▶ IDamageable.TakeHit ─▶ Health (corazones, invulnerabilidad)
                    └──▶ CombatFeel (parada, sacudida, chispas, sonido)
```

- **Todo lo que recibe golpes es un `IDamageable`**: Serena (`PlayerCombat`),
  los muñecos (`TrainingDummy`) y, en la Fase 2, las Pesadillas y los jefes.
- **Sin colisionadores de golpe permanentes**: mientras la ventana del golpe está
  abierta se pregunta a la física (`OverlapSphere` en la capa del equipo
  contrario) y se lanza un **rayo contra el escenario**: no se pega a través de
  paredes (trampa 108 de Godot).
- **Capas**: 6 `Player`, 7 `Enemy`, 8 `Pickup`. El escenario es `Default` y es lo
  que tapa los golpes.

## El tiempo lo marca la animación

En Godot el golpe conectaba por un tiempo fijo y la animación iba por su lado.
Aquí:

1. **Medir impactos** muestrea cada clip y busca el instante en que una mano o
   un pie llega más lejos por delante del cuerpo: `impactTime`.
2. Muchos clips de Mixamo tardan más de un segundo en llegar al golpe (el
   segundo puñetazo, a 1,13 s). Así que el clip **empieza ya avanzado**
   (`startTime`) para que la anticipación que se ve sea `targetWindup`.
3. La ventana de golpe se abre ±`activeHalfWidth` alrededor del impacto.

| Golpe | Empieza en | Anticipación | Total | Godot |
|---|---|---|---|---|
| Golpe 1 | 0,13 s | 0,14 s | 0,27 s | 0,29 s |
| Golpe 2 | 0,80 s | 0,14 s | 0,28 s | 0,29 s |
| Patada giratoria | 0,56 s | 0,22 s | 0,48 s | 0,53 s |
| Especial | 0,65 s | 0,50 s | 1,05 s | 1,30 s |

**Al cambiar un clip de ataque, volver a pasar «Medir impactos».**

## Reglas del GDD que implementa

| Regla | Cómo |
|---|---|
| Machacar siempre funciona | Combo de 3 con ventana de 0,8 s; la pulsación durante un golpe se guarda y encadena en cuanto pasa el impacto |
| Basta con acercarse y machacar | Auto-orientación (distancia + ángulo × 1,6, la fórmula de Godot) e **imán**: si el enemigo está a menos de 4,5 m pero fuera del golpe, salta hacia él |
| El especial es el espectáculo | 12 golpes lo cargan, invulnerable, 5 m de radio, rompe escudos, la barra late al llenarse. **No se carga a sí mismo** |
| Nunca se pierde | Con 0 corazones se marea 1,6 s y vuelve con la vida llena |
| 1,5 s de invulnerabilidad tras un golpe | `Health`, con parpadeo |

## Pendiente (Fase 1)

- Transformación (Timeline) y el nombre del ataque en pantalla al lanzar el especial.
- Zoom de cámara en el especial.
- Tiara Lunar (el tercer golpe la lanza; también es el primer poder de campo).
- Doble salto y agarrar cornisas.
- **Afinar jugando en el teléfono**: es el criterio de salida, y es tuyo.
