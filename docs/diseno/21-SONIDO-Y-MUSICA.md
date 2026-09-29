> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/21-SONIDO-Y-MUSICA.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Sonido y música

El juego lleva desde el principio **mudo salvo seis golpes**. Y el sonido no es
la capa de barniz que se pone al final: es la mitad de que pegar se sienta bien,
la mitad de que un sitio parezca un sitio, y lo único que puede decir «esto que
acabas de hacer estuvo bien» sin escribirlo en pantalla.

Este documento es el plan entero: de dónde sale cada cosa, qué suena en cada
momento y en qué orden hacerlo.

---

## 1. De dónde sacamos el material

Lo primero fue probar qué se puede descargar de verdad, porque media internet de
«recursos gratis para juegos» son páginas muertas o que piden cuenta. Lo de
abajo está **comprobado descargando**, no leído en una lista.

### Efectos: hacen falta REALISTAS, y eso cambia dónde buscar

Los de Kenney (CC0, ya descargados en `audio_fuentes/`) están bien hechos, pero
son **sonido de juego retro**: sintetizados, cortos, de dibujos animados. Para
menús y avisos sirven de sobra. Para que un guardián de piedra de dos metros y
medio pese, no — ahí hace falta grabación de verdad.

**Ya sustituidos** por grabación real (agosto 2026). Los de Kenney quedan solo
como despensa para menús y avisos, que es donde sí encajan.

#### También los puedo bajar yo

`tools/bajar_sonidos.py` busca en Freesound **con el filtro CC0 puesto** y baja
los previews públicos, que no piden cuenta:

```
python3 tools/bajar_sonidos.py "stone impact heavy" audio_fuentes/freesound 3
```

Son mp3 de calidad reducida y para efectos cortos en un móvil dan de sobra. La
URL del preview lleva un sufijo distinto por sonido y no se puede adivinar, así
que el script la saca de la propia página.

Sirve para lo genérico —silbidos, impactos, pasos—. Para lo que tiene que sonar
de una manera concreta sigue siendo mejor que lo elijas tú escuchando: buscar a
ciegas por palabras acierta la categoría, no el carácter.

#### Dónde bajarlos tú

**[freesound.org](https://freesound.org)** es la referencia para audio realista:
grabaciones de campo de gente que sale a grabar cosas. Pide cuenta gratis para
descargar —por eso lo bajas tú y yo lo ajusto—, y tiene filtro de licencia:
**pon siempre el filtro en CC0** y no hay que preocuparse de nada más.

Al buscar, dos cosas importan más que el nombre del sonido:

- **Filtrar por CC0** (a la izquierda, «License»). Lo demás obliga a citar y a
  llevar la cuenta de quién.
- **Ordenar por duración corta** para los golpes. Un impacto de ocho segundos
  con reverberación de sala trae el eco de otro sitio pegado.

Alternativas sin cuenta, por si te resulta más cómodo:

- **[Pixabay](https://pixabay.com/sound-effects/)** — realista y libre, sin
  registro. Menos variedad que Freesound pero más rápido de usar.
- **[Sonniss GDC Bundle](https://sonniss.com/gameaudiogdc)** — packs
  profesionales que regalan cada año. Son descargas de varios gigas y vienen sin
  ordenar, pero la calidad es de estudio.

#### La lista de la compra

Búscalos con estos términos —en inglés, que es donde está todo— y guárdalos como
vengan, con el nombre que traigan. Yo los recorto, les ajusto el volumen, los
convierto a `.ogg` y les pongo nombre.

**El guardián** — ✅ hecho, queda aquí como referencia de qué se buscó:

| Archivo | Qué buscar en Freesound | Cómo tiene que sonar |
|---|---|---|
| `guardian_paso` | `heavy stone footstep`, `boulder thud` | Grave y sordo, con algo de grava. **2 o 3 variantes** |
| `guardian_puno` | `rock impact`, `stone hit heavy` | Seco, corto, con peso |
| `guardian_suelo` | `boulder crash`, `earth impact rumble` | Lo más gordo del acto: golpe + retumbe que se aleja |
| `guardian_carga` | `stone grind`, `rock scrape low` | El roce mientras levanta los brazos |
| `guardian_despertar` | `stone door open`, `heavy rock grinding` | Largo, 4-5 s. Piedra que lleva siglos quieta |
| `guardian_encaje` | `rock chip`, `stone crack short` | Cuando le entra un golpe: piedra que se astilla |
| `guardian_derrumbe` | `rubble collapse`, `stone debris fall` | Al caer: se desmorona y luego silencio |
| `guardian_rugido` | `low rumble growl`, `deep earth groan` | Grave, sin ser animal. Es piedra, no bestia |

**El mundo** (los pasos ✅, el resto pendiente):

| Archivo | Qué buscar |
|---|---|
| `pasos_hierba` | `grass footstep` — 4 variantes |
| `pasos_tierra` | `dirt footstep` — 4 variantes |
| `salto` / `aterrizaje` | `jump cloth`, `landing thud soft` |
| `farol` | `fire whoosh small`, `torch ignite` |
| `estrella` | `magic chime sparkle` |
| `portal` | `magic portal hum low` |
| `puerta_magica` | `magic barrier dissolve` |

**Ambiente** (bucles largos, 1-2 min):

| Archivo | Qué buscar |
|---|---|
| `parque` | `park ambience birds`, `forest ambience light wind` |
| `agua` | `small stream loop` |
| `fuego_farol` | `small fire crackle loop` |

#### Qué hago yo cuando los tengas

Los dejas en una carpeta —`audio_fuentes/descargas/` vale— y me dices que están.
Yo me encargo de:

- Recortar el silencio del principio, que es lo que hace que un sonido llegue
  tarde aunque se dispare a tiempo.
- Igualar volúmenes entre ellos: descargados de sitios distintos, cada uno viene
  a un nivel y unos tapan a otros.
- Convertir a `.ogg`, que es lo que Godot usa sin recodificar.
- Ponerles nombre y enchufarlos donde toca.
- Comprobar que se oyen **en el altavoz del móvil**, que es lo que de verdad
  importa: lo que en cascos es un grave precioso, ahí no existe.

### Si hace falta algo que no esté

En este orden:

1. **[OpenGameArt.org](https://opengameart.org)** — descarga directa. Ojo: cada
   pieza tiene su licencia (CC0, CC-BY, GPL…), hay que mirarla una por una.
2. **[Pixabay](https://pixabay.com/sound-effects/)** — mucho y bueno, pero la
   descarga automática pide clave de API. Sirve para buscar tú a mano.
3. **[Musopen](https://musopen.org)** — clásica de dominio público. Para si algún
   día hace falta algo orquestal.
4. **[itch.io con filtro CC0](https://itch.io/game-assets/free/tag-sound-effects)**
   — packs de gente que hace juegos; calidad muy irregular.

**Lo que no vamos a usar:** nada de bibliotecas que pidan licencia por proyecto,
ni sonidos sacados del anime original. Esto es un regalo que no se publica, pero
meter material con dueño dentro del repositorio es empezar a acumular deuda que
un día hay que pagar.

---

## 2. Cómo está montado ahora

Ya existe [`AudioManager`](../src/autoload/audio_manager.gd) y no hay que
rehacerlo. Tiene:

- **Tres buses**: `Music`, `SFX`, `Voice`, cada uno con su volumen en ajustes.
- **Fundido cruzado** entre canciones (`play_music`), con dos reproductores que
  se turnan.
- **Un grupo de 16 reproductores** para efectos, que se van reciclando: así diez
  golpes seguidos no crean diez nodos ni se cortan entre ellos.
- **Variación de tono** automática en cada efecto (±10%). Es lo que evita el
  efecto ametralladora — el mismo sonido repetido idéntico veinte veces seguidas
  se vuelve insoportable, y con un poco de tono aleatorio deja de notarse.

Lo que hay grabado hoy: seis efectos de combate y las voces de Mercury. **Cero
música y cero ambiente.**

### Dónde va cada cosa

```
assets/audio/
  musica/      una carpeta por escena     ← vacío
  ambiente/    bucles largos de fondo     ← vacío
  sfx/
    combate/   golpes, daño, derrota
    mundo/     faroles, estrellas, portal, puertas
    jefe/      el guardián
    ui/        menús, avisos, objetivos
  voces/       ya está, por personaje
```

---

## 3. La música, escena por escena

La regla de fondo: **el parque es un sitio agradable donde a veces pasan cosas**,
no un juego de acción con ratos tranquilos. La música por defecto es la del
parque, y todo lo demás son excepciones que vuelven a ella.

| Momento | Cómo tiene que sonar | Cuánto dura |
|---|---|---|
| **Menú / título** | La melodía del juego, tranquila, reconocible. Es lo que suena mientras se decide jugar | bucle 1-2 min |
| **El parque** | Ligera, curiosa, sin prisa. Tiene que aguantar veinte minutos sin cansar: por eso poca percusión y nada de melodía pegadiza | bucle 2-3 min |
| **Combate en arena** | Entra al cerrarse la barrera y se va al abrirse. Más ritmo, misma paleta — que se note que es el mismo juego | bucle 1-2 min |
| **El claro del portal** | Más quieta y más abierta. Aquí no se pelea, se llega | bucle 1-2 min |
| **El guardián** | Lo único solemne del acto. Percusión pesada y lenta, como él | bucle 2 min |
| **Encuentro con Amy** | Se baja la del parque y entra algo cálido y corto, solo mientras se habla | 30-60 s |
| **Transformación** | No es música de fondo: es un momento. Sube, brilla y termina | 6-8 s |

### Las tres reglas de la mezcla

1. **Al hablar, la música baja, no se corta.** Cortarla deja un silencio que
   suena a error; bajarla a un tercio deja oír la voz sin que se note el cambio.
2. **Una canción por sitio, y se queda.** Cambiar de tema cada dos minutos marea.
   El fundido cruzado de un segundo que ya tiene el `AudioManager` es
   suficiente para que ningún cambio se note brusco.
3. **El silencio también se usa.** Antes de que el guardián despierte no debería
   sonar nada — dos segundos de nada hacen más por ese momento que cualquier
   redoble.

---

## 4. Los efectos, acción por acción

Marcados con ✅ los que ya suenan.

### Combate

| Qué | Cuándo | Dónde buscarlo |
|---|---|---|
| ✅ Golpe normal | Cada impacto | ya está |
| ✅ Golpe fuerte | Tercer golpe del combo | ya está |
| ✅ Daño recibido | Serena recibe | ya está |
| ✅ Bloqueo | El Cofrecito para un golpe | ya está |
| ✅ Especial | El ataque cargado | ya está |
| ✅ Derrota de enemigo | Un enemigo cae | ya está |
| Golpe al aire | Se ataca sin dar a nada | `impact-sounds`, los suaves |
| Salto | Al despegar | `rpg-audio/cloth*` |
| Aterrizaje | Al tocar suelo | `impact-sounds/footstep_*` |
| Pasos de Serena | Andando y corriendo | `impact-sounds/footstep_grass_*` |
| Agarrarse a una cornisa | Al engancharse | `rpg-audio/cloth*` |

Los pasos son los que más se notan aunque nadie los oiga conscientemente: sin
ellos, andar por el parque es flotar. Van con variación de tono y **cuatro
muestras alternándose**, o se convierten en un tictac.

### El guardián

| Qué | Cuándo |
|---|---|
| Piedra que se despierta | Los cinco segundos de `despertar` — roce grave, largo |
| Pasos | Cada pisada. Graves y con temblor; es lo que le da el peso |
| Puño | El golpe normal, al impactar |
| Puños contra el suelo | El ataque de área. Lo más gordo que suena en todo el acto |
| Aviso del área | Mientras el círculo crece — un zumbido que sube |
| Le entra un golpe | Piedra contra piedra, seco |
| Rugido de fase | Al cambiar de fase |
| Se derrumba | Al caer: piedra asentándose, y luego silencio |

### El mundo

| Qué | Cuándo |
|---|---|
| Farol encendido | Chispa y un tono que sube |
| Estrella recogida | Campanilla. Es el sonido que más veces se oye: tiene que gustar a la séptima vez |
| Rincón encontrado | Un acorde corto de logro |
| Puerta mágica que se abre | El paso que se libera |
| Portal despierto | Grave que crece, con brillo |
| Portal usado | El cambio de escenario |
| Objetivo cumplido | Jingle corto — `music-jingles` |
| Acto terminado | Fanfarria de verdad |

### Interfaz

Botón, confirmar, cancelar, abrir el panel de objetivos, pausa. Todo de
`interface-sounds`, y todo **corto**: un sonido de menú que dura más de un tercio
de segundo estorba.

### Ambiente

Va aparte de la música, en bucle y bajo. Es lo que hace que el parque no sea un
decorado:

- **Fondo de parque**: pájaros lejanos y aire entre las hojas.
- **Agua**: cerca del estanque, más alto cuanto más cerca. Con `AudioStreamPlayer3D`
  se resuelve solo.
- **Fuego de farol**: crepitar bajito al lado de cada farol encendido — así se
  oye cuál está encendido antes de verlo.

---

## 5. En qué orden hacerlo

Por cuánto cambian el juego, no por cuánto cuestan:

1. **Pasos y aterrizaje.** Es lo que más se nota y lo que ya está descargado.
   Andar sin sonido es lo que hace que un juego parezca una maqueta.
2. **Música del parque.** Veinte minutos de silencio de fondo es lo primero que
   se siente raro al jugar.
3. **Los del guardián.** Es el momento más importante del acto y va mudo.
4. **Los del mundo** — faroles, estrellas, rincones. Cada uno confirma que algo
   ha pasado, y ahora eso solo lo dice un cartel.
5. **Música de combate y de jefe.**
6. **Ambiente.**
7. **Interfaz.** Va la última porque es la que menos se echa de menos.

---

## 6. Lo que hay que cuidar

- **En el móvil, con el altavoz del teléfono.** Lo que en cascos es un grave
  precioso, ahí no existe. Todo tiene que funcionar en un altavoz malo y
  pequeño: eso significa nada de sonidos que vivan solo en los graves.
- **El volumen se mezcla al final, no al principio.** Cada efecto suelto suena
  bien; el problema aparece cuando suenan siete a la vez.
- **Ocho años.** Nada estridente, nada que asuste de golpe. El guardián impone
  por lo grave y lo lento, no por dar un susto.
- **Un sonido por acción, y siempre el mismo.** Si recoger una Estrella suena
  distinto cada vez, deja de significar «has recogido una Estrella».


---

## 7. Lo que ya está puesto

Procesado con `ffmpeg`: recortado, normalizado a un volumen común con
`loudnorm`, pasado a mono 44,1 kHz y a `.wav` — que Godot reproduce sin
decodificar, y en efectos cortos eso ahorra la latencia justo donde más se nota.

| Sonido | Tomas | De dónde |
|---|---|---|
| Paso del guardián | 3 | `heavy stone footstep` |
| Puño | 3 | `rock impact`, partido del archivo con variantes |
| Golpe al suelo | 3 | `guardian_suelo`, partido en tres |
| Encaje (le entra un golpe) | 2 | del mismo `rock impact`, más corto y más flojo |
| Despertar · carga · rugido · derrumbe | 1 c/u | grabaciones sueltas |
| Pasos de Serena | 4 | recortados de una grabación continua de hierba |
| Golpe, golpe fuerte, especial | 1 c/u | los sintéticos viejos, sustituidos |

**Varias tomas de cada cosa y se sortean.** El mismo golpe repetido idéntico
veinte veces se vuelve ruido de fondo por bueno que sea; con tres tomas y la
variación de tono que ya aplica el `AudioManager`, cada golpe suena parecido y
nunca igual.

Los pasos van **por distancia recorrida, no por tiempo**: así el ritmo sale solo
tanto andando como corriendo, y el pie cae donde suena.

### Lo que falta

Pasos de tierra (están cortados, sin usar — hace falta saber qué se pisa), y
toda la música.


## 8. El ambiente está hecho y APAGADO

Los `.mp3` de fondo están recortados y en `assets/audio/ambiente/`, y
`AmbienteNivel` está escrito. Pero **cuelga el juego**: con los siete
reproductores en marcha —bosque, agua y un fuego por farol— el nivel no llega a
pintar el primer fotograma. Sin error: se queda congelado.

La primera sospecha fue reconectar `finished` a `play()` —un sonido que no carga
termina en el mismo instante en que empieza y se llama a sí mismo sin parar— y
quitarlo no bastó. Queda por mirar si son los recursos `preload` compartidos
entre los siete reproductores.

Se deja montado y sin arrancar (`if false` en `nivel.gd`), no borrado. Lo demás
del sonido funciona.
