> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/23-MENUS.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Los menús

## Qué tenía que conseguir el menú principal

Que al abrirlo **apetezca jugar**. No es una pantalla de opciones con un botón
de jugar arriba: es el cartel del juego, y todo lo demás va detrás y más
pequeño.

El orden de la pantalla lo dice todo: fondo que se mueve → título grande →
botón de jugar → el resto. Lo que más ocupa es lo que más importa.

### El fondo va dibujado, no es una imagen

Una ilustración pesaría dos megas, habría que rehacerla para cada proporción de
pantalla —y los móviles las tienen todas— y estaría quieta. Dibujado ocupa nada,
se adapta solo y **se mueve**, que es la mitad de que una pantalla de inicio
parezca viva.

Tres capas a ritmos distintos: pétalos delante y rápidos, estrellas detrás y
casi quietas, la luna sin moverse. Esa diferencia de velocidades da profundidad
sin nada en 3D. Y todo **lento**: es la pantalla que se mira mientras se decide
jugar, y una animación rápida ahí cansa a los dos minutos.

La semilla del azar es fija, así que el fondo sale igual cada vez. Un fondo que
cambia de sitio en cada arranque se nota, y no para bien.

### El título y los botones

Título arriba, botones abajo. En un móvil la mano tapa la mitad de abajo, así
que lo que hay que **mirar** va arriba y lo que hay que **tocar** abajo, donde
llega el pulgar sin recolocar la mano.

Entrada escalonada: primero el fondo, luego el título bajando, luego los botones
uno detrás de otro. Es medio segundo largo y separa «se ha abierto una ventana»
de «ha empezado algo».

`JUGAR` **va directo a jugar**. Meter una pantalla de selección por delante en
la primera partida es poner un trámite entre el botón y el juego, y la primera
vez que se abre algo es cuando menos paciencia hay. Si ya hay partidas, el botón
pasa a decir `CONTINUAR` y aparece `Partidas` debajo.

## Las partidas guardadas

**Tres ranuras.** Una sola obliga a elegir entre seguir o empezar de cero, y a
los ocho años «empezar de cero» se pulsa por curiosidad y se pierde todo. Con
tres se puede trastear sin miedo. Tampoco más: tres caben en pantalla sin listas
ni desplazamientos.

Cada una enseña **lo que se ha hecho**, no un número de archivo — «★ 4 Estrellas
· 0 niveles · 12 min jugados». Un «Partida 2 — 14/08/2026» no dice de quién es
esa partida.

Para poder pintarlas sin abrirlas, cada archivo guarda un **resumen duplicado**.
Sin eso habría que cargar el progreso entero de tres partidas solo para escribir
tres líneas.

**Borrar es lo único que pregunta.** Es el único botón del juego que destruye
algo que ha costado horas. Y en la confirmación, «Mejor no» va primero y en
rosa: la salida segura es la que está más a mano.

## Los ajustes

Todo lo que hay **hace algo**, y se nota **al momento**: mover el volumen de
efectos suena un golpe de prueba en ese instante. Una barra que no responde
hasta salir de la pantalla no se entiende a ninguna edad.

No hay «Aceptar» ni «Cancelar»: se guarda al tocar cada cosa. Un menú que puede
perder lo que acabas de cambiar por salir mal es una trampa.

Cada interruptor lleva su explicación debajo. «Modo tranquilo» no dice qué hace,
y una opción que no se entiende no se toca — o se toca y luego no se sabe
deshacer.

## La pausa

Una pausa no es solo parar: es donde se va cuando **no se sabe qué hacer**. Por
eso «Qué me falta» —la misma lista que enseña el portal, reutilizada entera— va
lo primero después de seguir jugando.

«Guardar y salir» guarda sin preguntar. Preguntar «¿quieres guardar?» traslada
una decisión que nadie quiere tener que tomar.

En el móvil hay un botón de pausa arriba a la izquierda, pequeño y translúcido:
sin él no habría forma de pausar, guardar ni salir. Va donde no estorba — los
controles están abajo y los contadores a la derecha.

## Las tipografías

**Baloo 2** para títulos y botones: redondeada y gorda, se lee de un vistazo.
**Quicksand** para textos y números: geométrica y limpia, aguanta en pequeño.
Las dos de Google Fonts con licencia OFL. Son variables, así que un archivo trae
todos los grosores.

## Lo que queda

- **La música**, que es lo siguiente ahora que los menús existen.
- Que el botón de pausa se oculte durante los diálogos y las cinemáticas.
- Cuando cambie el sistema de vidas: `Partidas` guarda progreso, no la sesión —
  la vida y la posición no están en el formato a propósito, así que añadir
  puntos de guardado tocará `datos_de_sesion()` y no las ranuras.

## Los objetivos como misiones

Tocar una línea de la lista pone una **flecha que señala** hacia lo más cercano
de ese tipo. La lista decía *qué* falta pero no *dónde*, y el parque son
doscientos sesenta metros de lado: «busca seis faroles repartidos» sin más pista
es dar vueltas hasta cansarse.

No es un mapa ni un minimapa: **es dirección y distancia, no camino**. Llegar
sigue siendo cosa de quien juega, y por el camino se encuentran otras cosas.

Tres decisiones que la hacen funcionar:

- **Se elige, no aparece sola.** Una flecha permanente convierte el juego en ir
  detrás de una flecha, y entonces el parque deja de mirarse.
- **Se apaga al cumplir lo que señalaba.** Quedarse apuntando a algo ya hecho es
  peor que no apuntar.
- **Tocar el mismo objetivo otra vez la quita.** Es la forma natural de decir
  «ya no».

Cuanto más cerca está el destino, más pequeña y apagada se pone: es lo que dice
«ya casi» sin escribir una distancia en pantalla. Y recalcula el destino cada
fotograma en vez de guardarlo, porque los faroles se apagan y las Estrellas se
recogen — un destino guardado apuntaría a algo que ya no cuenta.

Respeta el ajuste **Flecha de ayuda**: si está apagado, avisa en vez de aparecer.
