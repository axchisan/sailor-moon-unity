> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/25-VIDAS.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# El sistema de vidas

## Lo que ya estaba bien y no se toca

**No hay game over.** Al quedarse sin corazones Serena se marea, pierde
Estrellas y vuelve a levantarse. Se reintenta el combate, nunca el nivel. Es el
primer pilar del GDD y sigue igual.

Lo que faltaba era todo lo que hay **entre** perder un corazón y quedarse sin
ninguno: no había forma de recuperarse, ni de guardar a propósito, ni de saber
dónde se reaparece.

## Los corazones

El GDD los prometía desde el principio —«los enemigos sueltan corazones a veces
al caer»— y no existían. Sin ellos la vida solo se recuperaba al terminar una
arena, y una pelea larga era cuesta abajo sin nada que hacer: cada golpe
recibido era definitivo hasta el final del combate.

**La probabilidad sube cuanta menos vida queda.** Con la barra llena no sale
ninguno; con un solo corazón salen el 55% de las veces:

| Vida | Probabilidad |
|---|---|
| 5 / 5 | nunca (no haría nada) |
| 4 / 5 | 20% |
| 3 / 5 | 28% |
| 2 / 5 | 36% |
| 1 / 5 | 47% |

Es una ayuda que no se anuncia ni se explica. Quien juega solo nota que las
peleas difíciles se pueden remontar — que es justo lo contrario de sentir que el
juego te está regalando algo.

Duran catorce segundos y parpadean los dos últimos. Sin límite, una arena larga
acaba llena de corazones sin recoger y dejan de significar nada; y desaparecer
de golpe se lee como que el juego se lo ha comido.

Su área de recogida es **generosa** (1,15 m): un corazón que hay que pisar con
precisión en mitad de una pelea no se recoge nunca.

## Los puntos de guardado

Un pedestal con un cristal girando, **uno justo antes de cada arena**. Al
tocarlo: cura del todo, guarda la partida y marca dónde reaparecer.

Antes el punto de reaparición lo ponía cada arena al empezar, en silencio. Eso
funciona pero no se puede **usar**: no se sabe dónde está, ni cuándo se ha
guardado, ni hay forma de decir «voy a dejarlo aquí». Un sitio que se ve, se
toca y responde resuelve las tres cosas a la vez.

**Curan siempre y sin límite de usos.** A los ocho años, un sitio de curación
que se gasta obliga a calcular cuándo usarlo, y calcular es exactamente lo que
no se quiere pedir aquí. Sin racionar, el sitio se lee como un refugio.

Se colocan **por código** a partir de dónde acabaron las arenas, en el borde por
el que se llega. El recorrido cambia cada vez que se toca el constructor, y un
punto de guardado clavado a unas coordenadas acabaría dentro de un seto.

## Lo que queda

- Que el cristal apagado se vea desde más lejos — hoy hay que estar cerca para
  distinguirlo del decorado.
- Reaparecer en el punto de guardado al marearse, no en el que puso la arena.
- Guardar también al cumplir un objetivo, no solo en los puntos.
