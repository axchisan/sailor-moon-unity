# El mundo — Juuban

> Dónde se juega, qué hay en cada sitio y cómo se abre. Las misiones de cada
> barrio están en [`06-MISIONES.md`](06-MISIONES.md); los poderes que abren
> secretos, en [`03-JUGABILIDAD.md`](03-JUGABILIDAD.md) §3.

## 1. El mapa

```
                         BAHÍA (acto III)
                              │
   TEMPLO HIKAWA ─── colina ──┤
        │                     │
        │              DISTRITO DE LAS LUCES
        │                     │
   PARQUE JUUBAN ──── CALLE JUUBAN ──── ESCUELA JUUBAN
                     (el centro)
                          ·
                  (el Espejo, en el cielo:
                   se entra al final)
```

- **Cada barrio es una escena** de Unity que se carga al cruzar su entrada (una
  calle con un arco o un cartel). La carga dura un par de segundos y tapa con
  una transición de estrellas. Así cada barrio puede ser grande sin pasarse
  del presupuesto del teléfono.
- **Se vuelve andando** o por los **Espejitos** reparados (viaje rápido).
- **Tamaño orientativo** de cada barrio: 150–250 m de lado de zona jugable,
  lo que se cruza corriendo en un minuto, con 3–5 sub-zonas reconocibles.

## 2. Calle Juuban — el centro

**Siempre abierta. Es la casa.** Pequeña (unos 120 m), densa, llena de gente.
Luz de mañana soleada; con la historia avanzada, atardecer.

| Sitio | Qué hay |
|---|---|
| **Casa Tsukino** | La habitación de Serena (guardar, decorar, armario, álbum, dormir); la cocina con su madre; Sammy en el salón |
| **Crown Game Center** | Andrew; recreativas; aquí está la máquina de **Sailor V** (minijuego, se abre con Mina). Punto de encuentro de las amigas |
| **Crown Fruit Parlor** | Lizzie; batidos y postres que curan; donde se juntan las chicas entre capítulos |
| **Joyería OSA-P** | La madre de Molly. Molly, la mejor amiga de Serena |
| **Boutique «Luna Llena»** | Tienda de ropa y accesorios (armario) |
| **Tienda de muebles y peluches** | Para la habitación |
| **Puesto de crepes** | El señor de los crepes; comida que cura |
| **Plaza de la fuente** | El **Espejito del centro** (viaje rápido, se repara en el prólogo) |
| **Tablón del barrio** | Lista de misiones de la gente, con dibujos |

**Qué cambia con la historia:** la calle empieza medio gris (mucha gente sin
sueño) y **recupera el color** a medida que se devuelven Estrellas. Las
tiendas abren cuando su dueño recupera el sueño (la boutique, por ejemplo,
está cerrada hasta que se hace su misión).

**Enemigos:** ninguno de día (es la zona segura). Alguna Grieta de noche.

## 3. Parque Juuban — Capítulo 1 (Amy)

**El barrio que ya existía en Godot, rehecho a mano y abierto.** Primavera,
cerezos, mañana.

| Sub-zona | Qué es |
|---|---|
| **La entrada y el paseo de los cerezos** | Camino ancho, bancos, faroles |
| **El estanque** | Agua, pasarela de madera, nenúfares, patos, barcas |
| **El parque infantil** | Columpios, tobogán, arenero |
| **El jardín de flores** | Macizos, fuente, invernadero pequeño |
| **La colina del mirador** | Vista de la ciudad; Amy con su ordenador; el **Espejito del parque** |
| **El claro del portal** | Donde está Temor, el Reflejo del Miedo |

- **Pesadilla nueva:** ninguna (se aprenden Peluchín, Cofrecito y Hadita).
- **Jefe:** **Temor**, el guardián de piedra del portal (el modelo del
  Guardián de Godot, con sus 11 animaciones).
- **Minijuego:** «Burbujas de Mercurio», en el mirador (ver [`07-ACTIVIDADES.md`](07-ACTIVIDADES.md)).
- **Secretos con poderes de después:** el islote del estanque (hielo), zarzas
  grises en el invernadero (fuego), la roca que tapa una cueva (fuerza), la
  copa del cerezo gigante (cadena).

## 4. Templo Hikawa — Capítulo 2 (Rei)

**Atardecer eterno de otoño**, hojas rojas, colina con escaleras.

| Sub-zona | Qué es |
|---|---|
| **La escalinata y los torii** | Subida larga en tramos, con descansillos |
| **El patio del templo** | El santuario, el fuego sagrado, el abuelo de Rei |
| **El bosque de arces** | Senderos, farolillos de piedra, cuervos |
| **El estanque de las carpas** | Puentecito rojo |
| **La casa de té** | Nicolás, el aprendiz del abuelo |
| **La cueva del eco** | Donde se esconde Rabieta |

- **Pesadilla nueva:** **Farolito** (un farolillo de papel que lanza
  chispitas de fuego y rebota). Enseña a esquivar.
- **Jefe:** **Rabieta**, el Reflejo del Enfado: grita, hace temblar el suelo,
  lanza llamaradas. Hay que esperar a que se canse para pegarle.
- **Minijuego:** «Lectura del Fuego», con Rei, en el fuego sagrado.
- **Cuervos Phobos y Deimos**: se han perdido; encontrarlos es una misión.

## 5. Escuela Juuban — Capítulo 3 (Lita)

**Mañana de día lectivo, pero vacía**: todos los alumnos están grises.

| Sub-zona | Qué es |
|---|---|
| **La entrada y el patio** | Árboles, bancos, la pista |
| **Los pasillos y las aulas** | Interior en dos plantas, taquillas, pizarras |
| **La biblioteca** | Estanterías altas (plataformas) |
| **El gimnasio** | Espacio grande: arena de combate |
| **La cocina del club** | Donde Lita hace pasteles sola |
| **La azotea** | Vista, viento, el **Espejito de la escuela** |
| **El invernadero del huerto** | Plantas, regaderas |

- **Pesadilla nueva:** **Pizarrín** (una tiza gigante que escribe barreras en el
  suelo). Enseña a rodear.
- **Jefe:** **Nadie**, el Reflejo de la Soledad: se vuelve invisible, hace que
  la gente no te vea. Hay que encontrarlo por su sombra.
- **Minijuego:** «La Pastelería de Lita» (primero en la cocina del club; tras
  su misión, en su propio puesto en la calle Juuban).

## 6. Distrito de las Luces — Capítulo 4 (Mina y Artemis)

**Noche de neón.** Escaparates, un escenario de conciertos, azoteas unidas por
cuerdas, un cine y la antena de televisión.

| Sub-zona | Qué es |
|---|---|
| **La avenida de los escaparates** | Tiendas, carteles luminosos, gente gris mirando al suelo |
| **La plaza del escenario** | Donde iba a actuar una cantante; arena grande |
| **Las azoteas** | Recorrido en altura con ganchos (cadena de Venus) |
| **El cine** | Interior oscuro, pantalla, butacas |
| **La antena** | Subida final; el **Espejito del distrito** |

- **Pesadilla nueva:** **Focote** (un foco de escenario que barre el suelo con
  un haz; si te pilla, te ralentiza). Enseña a moverse con el ritmo.
- **Jefe:** **Espejismo**, el Reflejo de «puedo sola»: hace copias falsas de
  Mina y de Serena. Solo se gana cuando las dos atacan juntas.
- **Minijuego:** la recreativa de **Sailor V** en el Crown (Mina *es* Sailor V,
  lo cual es un chiste que el juego no explica).

## 7. La Bahía — Acto III

**Atardecer con viento**, puerto, faro, muelles, un parque de atracciones
cerrado junto al mar. Aquí aparecen **Haruka y Michiru**.

- Zona más pequeña, sin misiones de gente hasta después del final.
- **Pesadilla nueva:** **Burbujón** (una pompa que se infla y estalla).
- **Enfrentamiento:** no hay jefe; hay un duelo de entrenamiento con Uranus y
  Neptune (no se puede perder: se interrumpe cuando se entiende la idea).
- **La Puerta del Tiempo** se abre desde el faro: Setsuna.

## 8. El Espejo — Acto final

**El interior del Espejo**: la calle Juuban al revés, flotando en el cielo,
hecha de cristal y de los sueños que la Dama ha guardado.

- Recorrido por fragmentos del Espejo; en cada uno se ve un trozo de la vida de
  Lira (lo que falta para entenderla).
- Tramo de retos con las cinco Sailor y cambio en tiempo real.
- **La Torre del Espejo** y la Dama (tres momentos, ver [`04-HISTORIA.md`](04-HISTORIA.md)).

## 9. Después del final

- La ciudad entera en color. Toda la gente con sus sueños.
- **El Jardín del Espejo**: una puerta nueva en el Parque lleva al jardín donde
  vive Lira. Se la puede visitar.
- **Rini** cae del cielo y trae misiones nuevas por toda la ciudad.
- Las Grietas siguen abriéndose (combate repetible y retos).

## 10. Día y noche (deseable)

Si el rendimiento y el tiempo lo permiten: en la habitación de Serena se puede
**dormir hasta la noche** o hasta la mañana.

- **De noche:** más Grietas, Pesadillas nocturnas, algún secreto y alguna
  persona que solo sale de noche (el astrónomo aficionado, el gato del tejado).
- **De día:** tiendas abiertas y la mayoría de misiones.

Si no cabe, cada barrio queda con su hora fija (la de la tabla) y se pierde
poco.

## 11. Ambiente (lo que hace que el mundo se sienta vivo)

- **Gente con rutina**: pasean, se sientan, compran; sus frases cambian cuando
  recuperan su sueño.
- **Color como marcador**: cada barrio empieza desaturado y se colorea a medida
  que se devuelven sus Estrellas. Al 100 %, suena una fanfarria y el barrio
  queda brillante para siempre.
- **Fauna**: mariposas, pájaros, patos, gatos, los cuervos del templo.
- **Música por barrio**, con una variante más triste mientras el barrio está
  gris y la alegre al recuperarlo.
- **Tiempo**: pétalos en el parque, hojas en el templo, viento en la bahía,
  neón y lluvia fina en el distrito.
