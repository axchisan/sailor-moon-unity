> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/13-NIVEL-1.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Nivel 1 — Parque Juuban, completo

> Plan de implementación para dejar el primer nivel **terminado de verdad**
> antes de tocar el segundo. Estado y avance al final del documento.

## Por qué se replantea

El blockout funciona y se recorre entero, pero es exactamente lo que su nombre
dice: pasillo → arena → pasillo → arena. Se acaba en diez minutos y **no hay
nada que decidir ni nada que descubrir**. Un beat 'em up para una niña de 8
años puede ser sencillo, pero no puede ser un pasillo.

Lo que se añade, en orden de cuánto cambia la sensación por cuánto cuesta:

| # | Qué | Qué aporta |
|---|---|---|
| 1 | **Estrellas de Sueño y secretos** | Motivo para mirar fuera del camino. Rejugabilidad |
| 2 | **Encuentro con una Sailor** | Convierte el nivel en historia. Empieza el reparto |
| 3 | **Un objetivo que no sea pelear** | Rompe el ritmo de arena-arena |
| 4 | **Un cruce con dos caminos** | Hay algo que decidir |
| 5 | **Decoración** | Lo último, cuando lo de arriba ya funciona |

La decoración va al final **a propósito**: un nivel que no se entiende en gris
tampoco se entiende con árboles.

---

## 1. Estrellas de Sueño

El coleccionable del juego. `GameManager.collect_star()` y el contador del HUD
ya existían desde la Fase 0, sin usarse.

**Siete por nivel**, repartidas así:

| Dónde | Cuántas | Idea |
|---|---|---|
| En el camino | 3 | Enseñan qué son. Imposible no cogerlas |
| Fuera del camino, a la vista | 2 | Se ven desde el camino pero hay que desviarse |
| Tras un salto o en una zona secreta | 2 | Para quien explore |

**Reglas de diseño:**

- **Con imán.** A los 8 años, calcular un salto para tocar exactamente un objeto
  es frustrante. Si te acercas lo suficiente, la estrella viene sola.
- **Nunca en un sitio donde puedas caerte.** Un coleccionable que castiga por
  intentar cogerlo enseña a no explorar.
- **Se ven de lejos.** Giran, flotan y brillan.

Al conseguir las siete: recompensa visible y se guarda en el progreso del nivel.

## 2. Encuentro con Sailor Mercury

Cada escenario presenta a un personaje del reparto. En el Nivel 1 es **Mercury**,
la primera del guion.

- Aparece **después de la Arena 2**, atrapada tras unos enemigos.
- Al acercarse: panel de diálogo breve, dos o tres frases.
- Se une al reparto (`EventBus.sailor_unlocked`), que quedará usable cuando esté
  el selector de personaje.

El sistema es **reutilizable**: un nodo `Encuentro` con su ficha de personaje y
sus frases. Los diez encuentros restantes son configurar, no programar.

## 3. Encender los faroles

En el tramo C, antes del guardián, hay **tres faroles de piedra apagados**. Hasta
que no se enciendan los tres, la puerta al guardián no se abre.

No hay que pelear: hay que **buscar**. Uno está a la vista, otro detrás de unos
arbustos y el tercero en la zona alta. Da un respiro entre la Arena 2 y el jefe,
y de paso justifica la caja de luz del `tōrō`, que ya lleva material propio.

## 4. El cruce

El tramo B ya no es un pasillo: es un tipo de tramo propio, `cruce`, que se
declara en el `RECORRIDO` como cualquier otro. Se construye como boca de
entrada ancha → dos ramas paralelas → boca de salida.

- **Rama derecha:** directa, con una Estrella a media altura de hombre. Irse
  por el camino corto no castiga.
- **Rama izquierda:** tres plataformas y las **dos Estrellas secretas** del
  nivel, con la pagoda de fondo.

Entre las dos ramas queda un hueco de 9 m sin suelo, poblado de cerezos: desde
una rama **no se ve la otra**, y elegir camino tiene gracia solo si lo que no
coges queda fuera de la vista.

### Coordenadas de rama

`punto_en_tramo()` acepta un quinto argumento, `rama`: −1 la larga, +1 la
directa, 0 el eje (las bocas). Las fichas de `ESTRELLAS`, `FAROLES` y
`ENCUENTROS` lo pasan con `ficha.get("rama", 0.0)`, así que todo lo anterior
sigue funcionando sin tocarlo.

## 5. Decoración

[`decorador_nivel.gd`](../src/level/decorador_nivel.gd), separado del
constructor a propósito: el constructor decide **por dónde se juega** y el
decorador **qué se ve**. Se puede borrar el nodo `Decorado` entero y el nivel
se sigue jugando igual, que es la prueba de que la separación está bien puesta.

- 267 props del kit sembrados por recorrido y arenas, en **11 draw calls**
- Fondo lejano a 60–200 m: montañas, pagoda, dos torii y cerezos gigantes, sin
  colisión ni sombra
- Disco de hierba de 210 m para que nada flote sobre el vacío y no se vea el
  canto del mundo contra el cielo
- Nada de esto tiene colisión: lo caminable ya lo definen los muros del
  blockout, y dar cuerpo a un arbusto solo añade sitios donde engancharse

### Presupuesto

Medido en el nivel entero, sin enemigos: **57 draw calls** de los 150 de
[`01-ARQUITECTURA.md`](01-ARQUITECTURA.md) §8. Para llegar ahí hubo que
agrupar también las cajas del blockout: eran 84 `MeshInstance3D` sueltos, o sea
84 draw calls, y ahora son 4 `MultiMeshInstance3D` (uno por color) con los
`StaticBody3D` reducidos a pura colisión. De 163 a 57.

---

## Duración objetivo

De 8-10 minutos a **18-25 minutos** la primera vez, contando exploración y
diálogo. Rejugándolo para completar las estrellas, menos.

No se alarga metiendo más oleadas: se alarga dando **cosas distintas que hacer**.

---

## Avance

| Parte | Estado |
|---|---|
| 1. Estrellas de Sueño | ✅ hecho |
| 2. Encuentro con Mercury | ✅ hecho (falta la voz) |
| 3. Faroles | ✅ hecho |
| 4. Cruce | ✅ hecho |
| 5. Decoración | ✅ hecho |

Queda fuera de este plan y pendiente: animar a Luna y Artemis (cola y patas),
ajustar el bus de audio `Voice` para que no tape la música, y revisar la altura
de la cámara del jugador, que va muy rasante y deja el camino casi fuera de
cuadro.
