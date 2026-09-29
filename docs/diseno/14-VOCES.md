> **Documento heredado del proyecto de Godot** (`sailor-moon/docs/14-VOCES.md`).
> El DISEÑO sigue vigente; las referencias a archivos `.gd`, nodos o escenas de
> Godot son historia. Cómo se hace en Unity: [`../ARQUITECTURA.md`](../ARQUITECTURA.md).

# Voces — guion para fish.audio

> Texto listo para pegar en [fish.audio](https://fish.audio), agrupado por
> personaje (una sesión por voz) y con las etiquetas de emoción ya puestas.

## Cómo encaja con el juego

Cada línea de diálogo lleva su etiqueta guardada **junto al texto**, en el mismo
recurso (`LineaDialogo.emocion`). No están en un documento aparte a propósito:
separados, se cambia una frase y el audio sigue diciendo la anterior, y nadie se
entera hasta que lo oye la jugadora.

El campo `audio` de cada línea puede quedar vacío. El diálogo funciona igual, en
silencio, así que se puede escribir y probar todo el guion antes de grabar nada.

## Etiquetas disponibles

**Tono:** `[angry]` `[sad]` `[embarrassed]` `[emphasis]` `[whispering]`
`[soft]` `[breathy]` `[excited]`

**Efectos:** `[laughing]` `[chuckling]` `[moaning]` `[clear throat]` `[sobbing]`
`[crying loudly]` `[sighing]` `[panting]` `[groaning]` `[crowd laughing]`
`[background laughter]` `[audience laughing]` `[pause]` `[long pause]`

## Cómo nombrar los archivos

```
assets/audio/voces/<dialogo>/<hablante>_<numero>.ogg
```

Por ejemplo `assets/audio/voces/n1_mercury/luna_01.ogg`. El número es el de la
lista de abajo, que coincide con el orden dentro de cada personaje.

> **Formato:** `.ogg` mejor que `.wav`. Godot lo carga igual y pesa una décima
> parte, y en el APK cada megabyte cuenta.

---

# Diálogo `n1_mercury` — El encuentro en el parque

16 líneas. Se dispara al acercarse a Ami, después de la Arena 2.

## Luna

```
1. [whispering] ¡Serena, espera! Hay alguien detrás de esos árboles…

2. [soft] No, tonta. Esa energía es distinta. Es… nuestra.

3. [emphasis] Ami, escúchame bien. Eso que sientes en el pecho no es miedo. Es tu poder despertando.

4. [emphasis] Di las palabras, Ami. Ya las conoces, aunque no sepas de dónde.

5. [soft] Completas no. Pero un poco menos solas. Vamos, queda camino.
```

## Serena

```
1. [excited] ¿Otra criatura? ¡Ya verás lo que le espera!

2. ¡Ami! ¿Qué haces aquí sola?

3. [excited] ¡Eso es justo lo que nos hace falta! Yo pego fuerte, pero pensar se me da fatal.

4. [excited] ¡Sailor Mercury! ¡Ahora sí que estamos completas!
```

## Ami / Mercury

```
1. [breathy] ¿Ho-hola? ¿Hay alguien ahí?

2. [embarrassed] Serena… menos mal que eres tú. Salí a estudiar al parque y de pronto todo se puso oscuro.

3. [sad] Los peluches de la feria se levantaron solos. Me escondí aquí y no me atrevía a salir.

4. ¿Mi… poder? Pero si yo solo sé resolver ecuaciones.

5. [chuckling] Eso sí es verdad.

6. [excited] ¡Por el poder del planeta Mercurio… transformación!

7. [soft] …Serena. Puedo verlo todo. Dónde están, cuántos son, por dónde vienen.
```

---

## Notas sobre el guion

**Por qué Ami no se lanza a pelear.** Es la lista del grupo, no la valiente. Que
esté escondida y muerta de miedo, y que su poder resulte ser *ver* en vez de
*pegar*, la define en tres frases sin explicar nada.

**Serena queda por debajo a propósito.** «Yo pego fuerte, pero pensar se me da
fatal» hace que la protagonista necesite a las demás. Si la protagonista se basta
sola, el reparto sobra.

**Luna es quien explica.** Al ser la que sabe, puede decir en voz alta lo que la
jugadora necesita entender sin que suene a tutorial.

---

## Estado

- [x] Las 16 líneas generadas y conectadas (67 s de diálogo, 1 MB en `.mp3`)
- [x] Escritura sincronizada con la voz
- [ ] Ajustar el volumen del bus `Voice` para que no tape la música

### Dos cosas que aprendimos moviendo los archivos

**El texto en pantalla se ajustó a lo que dicen los audios.** Se generaron con
«Amy» y «poder estelar», así que el guion se cambió para que coincida: si lo
escrito y lo que se oye no cuadran, canta muchísimo.

**Los nombres de fish.audio traen la hora y el principio del texto**, así que se
emparejan buscando un trozo distintivo de la frase, no por orden. Y hay que
comparar **sin acentos**: macOS guarda los nombres en Unicode descompuesto, y
las cinco líneas con tilde no casaban con el literal. Además fish.audio **trunca
el nombre**, así que la marca tiene que caber en lo que quede («resolver
ecuacione», sin la s).
