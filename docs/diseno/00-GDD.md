> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/00-GDD.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# GDD — Sailor Moon: El Cristal de los Sueños

> Documento de diseño. Proyecto personal, no comercial, regalo para una niña de 8 años.

## 1. Ficha técnica

| Campo | Valor |
|---|---|
| Título de trabajo | *Sailor Moon: El Cristal de los Sueños* |
| Motor | Godot 4.7.1 (GDScript) |
| Género | **Acción / beat 'em up ligero en 3D** con exploración breve entre combates |
| Perspectiva | Tercera persona, cámara orbital que se fija en la arena durante el combate |
| Plataforma principal | Android (touch), dispositivo moderno con Vulkan |
| Plataformas secundarias | macOS y Windows (teclado + gamepad) |
| Público | Niña de 8 años, hispanohablante, poca experiencia con videojuegos |
| Duración objetivo | 60–90 min de campaña, sesiones de 10–15 min |
| Idioma | Español (sistema de traducción preparado por si acaso) |
| Estilo visual | 3D con *cel shading* estilo anime (toon + outline) |

## 2. Pilares de diseño

1. **Imposible perder de forma frustrante.** No hay *game over*. Si te quedas sin corazones, Serena "se marea", pierde estrellas y reaparece en la misma arena con la salud llena. El combate se reintenta, nunca el nivel.
2. **Machacar un botón tiene que sentirse increíble.** Todo el presupuesto de esfuerzo va al *feel* del golpe: pausa de impacto, partículas, sonido, retroceso. Un beat 'em up es un 20% de sistemas y un 80% de sensación.
3. **La transformación y el ataque especial son el espectáculo.** Son los dos momentos que va a querer repetir. Deben verse desproporcionadamente bien respecto al resto.
4. **Los enemigos atacan de verdad y se derrotan.** ⚠️ *Pilar cambiado el 2026-08-04 a petición directa de la jugadora.* Originalmente los enemigos se "purificaban" y se quedaban de adorno como amigos; al probarlo pidió enemigos más hostiles y que desaparecieran al vencerlos. **Sigue sin haber nada desagradable:** el enemigo sale despedido, gira, se encoge y se deshace en un estallido de estrellas. Es la mejor fuente de diseño que existe —la propia jugadora— y manda sobre cualquier suposición previa.
5. **Se juega con dos pulgares, y uno solo machaca.** Joystick virtual + botón de ataque grande. Sin bloqueo, sin combos que memorizar, sin *timing* estricto.

## 3. Temática y tono

Referencia estética: la etapa más luminosa de la franquicia (arco *SuperS*, el de los sueños). Colores pastel, brillo, purpurina, música alegre. El conflicto es "alguien está robando los sueños bonitos de la gente", no "alguien quiere destruir el mundo".

**Nombres:** doblaje latinoamericano — Serena, Rei, Amy, Lita, Mina, Luna, Darién — con el nombre de Sailor en las presentaciones.

## 4. Guion / estructura narrativa

### Premisa

La **Dama del Espejo Roto** roba los *Cristales de Sueño* de los habitantes de Tokio. Sin su sueño, la gente se queda apagada y gris. Luna despierta a Serena: solo el Cristal de Plata puede devolver los sueños, pero está debilitado y se recarga con las **Estrellas de Sueño** que quedaron esparcidas al romperse el espejo.

Cada nivel es un barrio donde una amiga está perdiendo su sueño. Al limpiar el barrio de monstruos y derrotar al guardián del espejo, esa amiga despierta y se une al equipo como Sailor jugable.

### Acto 1 — Despertar

**Nivel 1 · Parque Juuban** *(tutorial)*
Serena persigue a Luna, aprende a moverse y saltar. Aparece el primer monstruo del espejo, Luna le da el broche: **primera transformación**. Se aprende a atacar. Tres arenas cortas de dificultad ascendente y un guardián muy suave.

**Nivel 2 · Templo Hikawa** — *Rei / Sailor Mars*
Escaleras de piedra, faroles, cuervos, hojas de otoño. Introduce enemigos que atacan a distancia. Rei barre el patio sin ganas hasta que despierta. Desbloquea **Sailor Mars** (fuego: daño en área).

### Acto 2 — El equipo

**Nivel 3 · Escuela y biblioteca** — *Amy / Sailor Mercury*
Pasillos, aulas, estanterías. Introduce enemigos con escudo que hay que romper con el ataque especial. Desbloquea **Sailor Mercury** (agua: ralentiza a los enemigos).

**Nivel 4 · Arcade Crown** — *Lita / Sailor Jupiter*
Neón, máquinas recreativas, arena elevada con bordes de los que se cae. Enemigos rápidos en grupo. Desbloquea **Sailor Jupiter** (rayo: lenta pero devastadora).

**Nivel 5 · Pasarela y teatro** — *Mina / Sailor Venus*
Escenario giratorio, cortinas, focos. Arenas con peligros ambientales (focos de luz oscura que barren el suelo). Desbloquea **Sailor Venus** (cadena: alcance largo, engancha enemigos lejanos).

### Acto 3 — El sueño

**Nivel 6 · Torre del Espejo Roto**
Gauntlet de arenas que reutiliza a todos los enemigos del juego, con **cambio de Sailor en tiempo real** desde un menú radial. La Dama del Espejo Roto en tres fases, cada una vulnerable al poder de una Sailor distinta (fuego → hielo → rayo), lo que obliga a usar el equipo completo. Final: las cinco combinan poderes, los sueños vuelven, celebración en el parque con toda la ciudad despertando.

### Post-créditos
Aparece Chibiusa — o mejor, un personaje sorpresa con el nombre de tu prima, desbloqueable como Sailor extra. Gancho barato y muy efectivo.

## 5. Combate — el núcleo del juego

### 5.1 Verbos

| Acción | Android | Desktop |
|---|---|---|
| Mover | Joystick virtual (abajo izquierda) | WASD / stick izq. |
| Cámara | Arrastrar mitad derecha (auto-encuadre en combate) | Mouse / stick der. |
| **Atacar** | Botón grande (abajo derecha) — se machaca | Click izq. / X |
| Saltar / esquivar | Botón secundario | Espacio / A |
| **Especial** | Botón que brilla cuando está cargado | Click der. / Y |
| Transformar | Botón contextual, solo cuando el juego lo permite | Q |
| Cambiar Sailor | Menú radial (solo Nivel 6) | Tab |

### 5.2 Combo básico

Machacar el botón encadena **3 golpes**: puñetazo → puñetazo → patada giratoria con retroceso. La ventana para encadenar es de **0,8 s**, absurdamente generosa a propósito: a los 8 años el ritmo no es fiable, y el combo debe salir *siempre* que se machaque.

No hay combos direccionales, ni cancelaciones, ni *juggling*. Un botón, tres golpes, se repite.

### 5.3 Auto-orientación (crítico)

Al atacar, el personaje **gira automáticamente hacia el enemigo más cercano** dentro de un cono frontal de 120° y 4 m. Sin esto, un beat 'em up en 3D es injugable para una niña: apuntar en 3D con un joystick virtual es la barrera número uno. Con esto, basta con acercarse y machacar.

### 5.4 Ataque especial

- Una **barra de energía** se llena golpeando enemigos (~12 golpes la llenan).
- Al llenarse, el botón brilla y suena un *ding*. Es imposible no darse cuenta.
- Al pulsarlo: pausa breve, zoom de cámara, el nombre del ataque en pantalla grande (*"¡Por el poder del prisma lunar!"*), y un ataque en área que barre a todos los enemigos cercanos de una vez.
- Es invulnerable durante la ejecución: también funciona como botón de pánico cuando se agobia.

Este es el momento que va a querer repetir una y otra vez. Merece que le dediquemos VFX de más.

### 5.5 Movesets por Sailor

Todas comparten el **mismo esqueleto humanoide y las mismas animaciones**. Se diferencian por VFX, proyectiles, alcance y ritmo — no por animación nueva. Esta decisión es la que hace viable el beat 'em up con un pipeline gratuito.

| Sailor | Alcance | Ritmo | Rasgo | Especial |
|---|---|---|---|---|
| **Moon** | Medio | Normal | Equilibrada; 3er golpe lanza la Tiara Lunar que vuelve como bumerán | Área circular grande |
| **Mars** | Corto | Rápido | Golpes que dejan llamas en el suelo | Cono de fuego frontal |
| **Mercury** | Medio | Normal | Los golpes ralentizan a los enemigos 3 s | Congela a todos en pantalla |
| **Jupiter** | Corto | Lento | Daño alto, buen retroceso, rompe escudos | Rayo en cadena entre enemigos |
| **Venus** | Largo | Normal | Cadena que alcanza y atrae enemigos lejanos | Haz que atraviesa la arena |

### 5.6 Enemigos

Tres arquetipos, con variantes cosméticas por nivel (mismo esqueleto, distinto color y sombrero — así el coste de arte no se multiplica):

| Arquetipo | Comportamiento | Contra |
|---|---|---|
| **Peluchín** (básico) | Se acerca y golpea. Telegrafía 0,75 s brillando en rojo | Machacar |
| **Hadita** (a distancia) | Hada de espejo roto: se mantiene lejos y lanza esquirlas brillantes lentas y esquivables. Huye si te acercas | Perseguirla |
| **Cofrecito** (escudo) | Se protege de frente; hay que romperle el escudo | Ataque especial o Jupiter |

**Reglas de compasión, no negociables:**
- Nunca más de **4 enemigos atacando a la vez**. Los demás esperan en un anillo alrededor y solo entran cuando se libera un turno (sistema de fichas de ataque, el patrón clásico del género). Se subió de 3 a 4 al pedir la jugadora más intensidad.
- Todo ataque enemigo se telegrafía **con brillo rojo** antes de conectar. Se bajó de 1,2 s a 0,75 s para subir la tensión, pero el aviso NUNCA se quita: es la regla de compasión que queda intacta.
- Los enemigos nunca atacan por la espalda sin avisar.
- Al recibir daño hay 1,5 s de invulnerabilidad con parpadeo.

### 5.7 Sensación de impacto (*game feel*)

Lo que separa un beat 'em up bueno de uno malo. Todo esto es barato de implementar y es donde más rinde el esfuerzo:

| Efecto | Detalle |
|---|---|
| **Hit stop** | Congelar 60–100 ms al conectar (`Engine.time_scale = 0.05`). Es el truco más potente que existe |
| **Screen shake** | Sacudida sutil, más fuerte en el 3er golpe y en el especial |
| **Partículas** | Estrellas y corazones rosas en el punto de impacto |
| **Retroceso** | El enemigo sale despedido; el 3er golpe lo manda lejos |
| **Sonido** | *Pop* alegre con pitch aleatorizado ±10% para que no canse |
| **Flash** | El enemigo parpadea en blanco 80 ms |
| **Derrota** | El enemigo sale despedido, gira, se encoge y estalla en estrellas |

### 5.8 Estructura de nivel

Cada nivel es: **tramo corto de exploración → arena de combate → tramo → arena → tramo → guardián**.

Las arenas cierran con una barrera mágica rosa y se abren al limpiar las oleadas (2–3 oleadas por arena). Los tramos entre arenas son para respirar, recoger chispas y ver el escenario. Esto además abarata la producción: las arenas son pequeñas y contenidas, no mundos abiertos.

## 6. Economía y progresión

- **Estrellas de Sueño:** 3 por nivel — una por arena limpiada, una por el guardián, una por encontrar el secreto del nivel.
- **Chispas:** las sueltan los enemigos al caer. ~50 por nivel. Se gastan en el armario.
- **Corazones:** 5 de salud (más generoso que un beat 'em up normal). Los enemigos sueltan corazones a veces al caer.
- **Derrotados:** contador de enemigos vencidos en el nivel. *"¡34 derrotados!"*
- **Armario:** lazos, colores de falda y accesorios comprables con chispas. La rejugabilidad más barata y más efectiva a esta edad.

## 7. Accesibilidad para 8 años

- Cero texto obligatorio: los diálogos importantes llevan voz o iconos. Fuente grande y redondeada.
- Flecha guía siempre disponible apuntando al objetivo.
- Sin temporizadores ni presión.
- Botones táctiles de mínimo 96×96 px reales, bien separados.
- Pausa que congela todo de verdad.
- Guardado automático constante, sin ranuras.
- **Modo asistido opcional** (activable desde el menú, sin decírselo): más salud, enemigos más lentos. Por si el playtest revela que se atasca.

## 8. Nota legal (una vez y seguimos)

Sailor Moon es propiedad de Naoko Takeuchi / Kodansha / Toei Animation. Como regalo personal que no se distribuye ni se monetiza, en la práctica no hay problema. La línea a no cruzar: publicarlo en Google Play, itch.io o cualquier tienda, ni monetizarlo. Además, varias herramientas de IA (Meshy, Tripo) licencian sus salidas gratuitas como CC BY 4.0, lo que apunta en la misma dirección. Premisa del proyecto: **build privada, instalada por sideload en el dispositivo de tu prima.**
