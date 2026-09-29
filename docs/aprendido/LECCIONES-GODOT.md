# Lo aprendido en Godot, destilado

El proyecto de Godot (`../sailor-moon`, `docs/ESTADO.md` §6) apuntó **133
trampas**. Unas 30 eran de Godot y aquí no existen. Las otras ~100 eran de
cómo se hacía el juego, y valen igual en Unity. Esto es su resumen, ordenado
por tema, con lo que cambia en este proyecto. El número entre corchetes es la
trampa original, por si hace falta el detalle.

## 1. Generar el mundo por código

**La lección más cara.** Casi la mitad de las trampas vienen de colocar cosas
por cálculo en vez de a mano:

- Props medio enterrados porque el tramo se registraba por su eje y no por su
  superficie [29]; árboles flotando porque la altura del camino no vale fuera
  del camino [36]; 334 props montados unos sobre otros porque ninguna siembra
  sabía de las demás [44]; el estanque acabó debajo del parque porque colgaba
  de un centro que se movió [64, 79]; cerezos en mitad del puente [80]…
- Cada arreglo añadía una regla (registro de ocupación, rayos al suelo,
  «reservar sitio», abanicos de búsqueda) y cada regla traía su propio fallo.

**En Unity:** el nivel se hace en el editor ([`../ARQUITECTURA.md`](../ARQUITECTURA.md) §1).
Lo que se ve es lo que hay. Si un andamio siembra algo, se revisa a ojo y se
corrige a mano; no se añaden reglas para que «se coloque solo bien».

## 2. Rendimiento en el móvil

Todo esto está medido en el HONOR (Adreno 619) y es de la GPU, no del motor:

- **El Mac no mide nada** [115]: 180 fps con todo y 156 sin dibujar nada.
- **Saber qué cuello es antes de optimizar** [66]: geometría, píxeles o CPU son
  tres problemas distintos. Se atacaron los triángulos (358k → 144k) y los fps
  no se movieron porque lo caro eran los píxeles. La prueba que lo decide:
  bajar la resolución; si los fps se disparan, es de píxeles.
- **El suelo ES el fotograma, y lo caro es iluminarlo** [119]: quitar el
  terreno rendía lo mismo que quitar el mundo entero; el mismo terreno sin
  iluminar, +108 %. El patrón de su shader apenas pesaba (+4 %).
- **En una GPU de móvil el coste va a escalones** [120]: muestras de textura y
  registros deciden cuántas ondas caben; recortar poco a poco no se nota, hay
  que cruzar el umbral.
- **El pase de sombra puede costar más que la imagen** [50]; mirar pantalla y
  sombra por separado. Una cascada y alcance corto.
- **Una multimalla es un objeto con UNA caja** [51]: trocear por parcelas o no
  se descarta nunca. *(En Unity: el agrupado estático y el instanciado
  respetan el descarte por objeto; no agrupar a mano sin pensar en esto.)*
- **`sin()` por píxel es carísimo** [52, 67]; hornear el ruido a textura [68].
- **FXAA fue lo más caro medido**: relee el fotograma. *(Aquí: MSAA 4x, que en
  una GPU por tiles se resuelve dentro del tile.)*
- **Nada de efectos a pantalla completa**: el ajuste de color se hornea en los
  colores de origen, no se aplica al fotograma.
- **Bajar la resolución emborrona el contorno del cel shading** [63]; decisión
  de aspecto del usuario. Pero **no afecta a la interfaz** [127] y es la palanca
  más fuerte: a 0,6 iba a 60 clavados. Hay FSR sin probar.
- **El teléfono se estrangula por calor** [122, 131]: ±40 % en el mismo sitio y
  −22 % tras una hora. Intercalar la referencia entre pruebas y mirar el estado
  térmico. Los fps topan en 60 por vsync (que en Android no se quita) [121]:
  medir el tiempo de GPU, no los fps.
- **Si los draw calls cambian entre filas, la cámara se movió** [123].
- **Los tirones son estrenos, no zonas** [130, 133]: compilar shaders y
  pipelines la primera vez que se ve cada material. El arranque en frío eran
  15 s: 9,5 de cargar recursos y 4,8 de estrenar el nivel. *(En Unity: cargar
  mientras se ve el menú y precalentar shaders en la pantalla de carga.)*
- **Medir el arranque en la máquina equivocada ordena mal las prioridades** [118].

Números de referencia y cómo se mide aquí: [`../RENDIMIENTO.md`](../RENDIMIENTO.md).

## 3. Aspecto (cel shading)

- **Rim de MToon = emisión** [110]: un rim blanco sumado a la emisión es un
  «aura blanca» que no apaga nada. *(El toon de aquí multiplica el rim por la
  luz.)*
- **La normal doblada hacia arriba borra el escalón** [111]: si el personaje se
  ve plano, mirar eso antes que la paleta.
- **Dos velos atmosféricos se suman** [112]: niebla + perspectiva aérea lavaban
  media pantalla. Una sola.
- **Filmic desatura** [113]: tonemap lineal (aquí, sin HDR ni tonemap).
- **Un perfil de móvil que cambia el aspecto hay que mirarlo con captura** [114].
- **Borde de sombra en zigzag = aliasing, no estilo** [73]: filtro suave.
- **Dos superficies casi coplanares parpadean** [31, 53, 54]: separar de verdad
  o que una tape a la otra.

## 4. Modelos, animación y pipeline

- **hi3d.ai entrega ~2 M de triángulos y 4K**: paso obligado por Blender
  (`blender_preparar_modelo.py` en Godot) [22]. Presupuesto: 25k por
  personaje, 5k por enemigo.
- **Tripo y Meshy cobran por EXPORTAR** [21]: al probar una herramienta, lo
  primero es bajar un archivo.
- **Geometría de más pegada al personaje** (el bonsái de Jupiter, el coral de
  Venus): separar ANTES de decimar [19].
- **Comprobar el original antes de perseguir un defecto** [23, 28]: las botas de
  Uranus venían así de fábrica. Los FBX tal como salen de Mixamo están en
  `../sailor-moon/Models/rigged_originales/`.
- **Mixamo exporta con jerarquías distintas según el día** [84] y **«In Place»
  no quita la altura** [85]. *(En Unity: Humanoid resuelve los nombres, y la
  altura de la raíz se descarta por configuración, ver `ModelImportRules`.)*
- **El pintado automático de pesos falla en silencio** [33]: contar vértices sin
  grupo antes de exportar.
- **Las texturas sin usar se arrastran a la build** [102, 117]: al excluir algo,
  excluir lo que arrastra.
- **Marcar geometría en VERDE, no en rojo** [25]: media Sailor va de rojo.

## 5. Juego y lógica

- **Una señal que nadie escucha no da error** [90]; **un `match` sin caso por
  defecto se traga datos** [56, 57]. *(En C#: `switch` con `default` que avise,
  eventos comprobados en tests.)*
- **Dos números que describen lo mismo acaban separándose** [91]; escribir uno
  y derivar el otro. **Si algo se genera, su cuenta se genera con ello** [97].
- **Un recurso compartido se cambia para todos** [92, 125]. *(En Unity:
  `sharedMaterial` vs `material`, ScriptableObjects modificados en Play Mode.)*
- **Toda condición de avance que dependa de que un enemigo muera necesita una
  salida** [95]; **todo sitio que encierre necesita su punto de vuelta** [104];
  preguntar dónde ESTÁ la jugadora, no qué está cerrado [105].
- **Los disparadores de una escena son cuándo Y dónde** [106].
- **Toda barrera infranqueable para los pies lo es para los golpes** [108].
- **Antes de retocar un arreglo que «no funcionó», buscar quién más hace lo
  mismo** [109].
- **Un estado que se devuelve a sí mismo cuelga el juego** [99]: límite de
  cambios por fotograma en la máquina de estados.
- **Para probar el final hay que poder saltar al final** [98]: atajos de
  desarrollo desde el primer día.
- **Sin pantalla de carga, cargar parece un cuelgue** [107], y la barra no debe
  mentir.
- **Leer en voz alta lo que sale por pantalla** antes de buscar un bug en un
  contador [96].
- **Un control de interfaz se come el toque** [34]. *(En Unity: `raycastTarget`
  a false en todo lo decorativo.)*

## 6. Diseño (lo decidió la jugadora o el usuario)

Está entero en [`../diseno/00-GDD.md`](../diseno/00-GDD.md). Lo que no se toca:

- Sin *game over*: se marea y reaparece en la misma arena.
- Máximo 4 enemigos atacando a la vez; todo ataque avisado con brillo rojo
  (0,75 s); 1,5 s de invulnerabilidad tras recibir daño.
- Combo de 3 golpes con ventana de 0,8 s; auto-orientación al enemigo más cercano.
- Los enemigos se eliminan de verdad (lo pidió la jugadora), sin nada desagradable.
- Joystick flotante; botones de al menos 96×96 px.
- Todo en español; nombres del doblaje latino (Serena, Rei, Amy, Lita, Mina).
