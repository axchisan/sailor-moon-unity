# Contenido — cuánto hay, cuántas horas y qué hay que producir

## 1. Inventario del juego

| Qué | Cuántos |
|---|---|
| Zonas | Calle + 4 barrios + Bahía + el Espejo + Jardín (post) |
| Capítulos de historia | Prólogo + 6 + epílogo |
| Misiones de la gente | 30 (+ 6 de Rini después del final) |
| Estrellas de Sueño | **111**: 35 de historia, 30 de misiones, 40 ocultas, 6 de Rini |
| Sailor jugables | 5 (+ 5 extra después del final) |
| Jefes | 4 Reflejos + la Dama + duelo con las Outers |
| Tipos de Pesadilla | 7 (Peluchín, Cofrecito, Hadita, Farolito, Pizarrín, Focote, Burbujón) + versiones grandes y doradas |
| Minijuegos | 4 grandes + 3 pequeños |
| Cartas | 110 |
| Ropa y accesorios | ~40 |
| Muebles y adornos | ~50 |
| Recuerdos de Luna | 12 |
| Retos | 3 por barrio (15) |

## 2. De dónde salen las horas

| Bloque | Cálculo | Horas |
|---|---|---|
| Historia | 25 min de prólogo + 4 × 45 min + 40 + 45 min | ~4,5 |
| Misiones | 30 × 5 min de media | ~2,5 |
| Estrellas ocultas y secretos | 40 × 1,5 min + volver a barrios con poderes | ~1 |
| Minijuegos | 4 × 3 niveles × 5 min | ~1 |
| Grietas y retos | Repetible; 15 retos × 3 min | ~0,75 |
| Después del final | Rini + completar | ~1 |
| **Total al 100 %** | | **~10** |
| **Solo historia** | | **~4,5** |

Para una niña de 8 años el ritmo real es más lento que esta cuenta (explora,
repite lo que le gusta): estas horas son un **mínimo**.

## 3. Premios por completar

| Se completa | Premio |
|---|---|
| Un barrio al 100 % | Fanfarria, mueble especial, carta dorada del barrio |
| Página del álbum «Las Sailor» | **Uranus jugable** |
| Página «Gente de Juuban» | **Neptune jugable** |
| Página «Pesadillas» | **Pluto jugable** |
| Página «Recuerdos» | **Saturn jugable** |
| Misiones de Rini | **Chibi Moon jugable** |
| Álbum entero | Carta secreta y final extendido (una escena más con Lira) |
| Cristal al brillo 10 | Aura dorada |

Las Outers y Chibi Moon ya están modeladas y rigueadas: usan las mismas
animaciones. Solo falta su estilo de combate (efectos) y su poder de campo, que
puede repetir alguno de los existentes con otro aspecto.

## 4. Qué hay que producir

### Ya existe (del proyecto de Godot)

- Las 11 Sailor rigueadas (forma Sailor), Luna, Artemis.
- Peluchín, Cofrecito, Hadita y el guardián (Temor) con 11 animaciones.
- 25 animaciones de Mixamo compartidas.
- Kit de parque: 41 props y 27 superficies.
- Banda sonora del parque, combate, jefe, menú, créditos, encuentro; ~60 efectos.
- Voces de Mercury, Serena y Luna del encuentro del parque.

### Hay que hacer

| Qué | Cuántos | Cómo | Prioridad |
|---|---|---|---|
| **Gente de la ciudad** (civiles con misión) | ~30 | hi3d.ai → Blender (reducir a 8–12k) → Mixamo | Núcleo |
| **Figurantes** (gente que pasea) | 6 modelos × 3 colores | Igual; reutilizados | Núcleo |
| **Modelos de calle** de las 5 amigas y Darién | 6 | Igual | Deseable (sin ellos, forma Sailor en todo) |
| **Ropa de Serena** (conjuntos de calle) | 8 | Un modelo rigueado por conjunto, mismo esqueleto | Deseable |
| **Accesorios** (lazos, gafas…) | ~20 | Blender, pegados a un hueso | Núcleo |
| **La Dama / Lira** | 2 (Dama y Lira) | hi3d.ai → Mixamo | Núcleo |
| **Reflejos nuevos** (Rabieta, Nadie, Espejismo) | 3 | hi3d.ai; animación de Mixamo o procedural | Núcleo |
| **Pesadillas nuevas** | 4 | hi3d.ai o Blender; animación procedural | Núcleo |
| **Kits de barrio** (calle, templo, escuela, distrito, bahía, espejo) | 6 | Blender por script (como el kit del parque) + hi3d.ai para lo singular | Núcleo |
| **Interiores** (casa, Crown, cocina, aulas, cine) | 6 | Kit modular | Núcleo |
| **Animaciones nuevas de Mixamo** | ~20 | Gestos de NPC, bailar, cocinar, cargar, saludar, sentarse | Núcleo |
| **Música** | ~12 piezas | Una por barrio (versión gris y alegre), minijuegos, acto final, Lira | Núcleo |
| **Voces** | Las escenas clave | fish.audio como en Godot; el resto, texto con sonidito por personaje | Deseable |
| **Ilustraciones** de la apertura y de los recuerdos | 5 + 12 | Imagen generada, estilo pastel | Deseable (sin ellas, escenas con los modelos) |
| **Dibujos de cartas** | 110 | Capturas de los modelos con marco, más algunas ilustraciones | Núcleo (capturas) |

### Lo que más cuesta y cómo se abarata

1. **La gente de la ciudad** es lo más caro en cantidad. Se abarata con un
   presupuesto de polígonos menor (8–12k, no 25k), una sola textura, y
   compartiendo las animaciones de Mixamo con las Sailor (mismo esqueleto).
2. **Los kits de barrio**: el del parque ya demostró que un kit por script de
   Blender sale rápido. Cada barrio necesita unas 30–40 piezas.
3. **La música**: dos versiones por barrio (gris y alegre) pueden ser la misma
   pieza con instrumentos distintos.

## 5. El 100 % (lista para la jugadora)

- [ ] Las 111 Estrellas devueltas
- [ ] Los 5 barrios al 100 % de color
- [ ] Las 30 misiones y las 6 de Rini
- [ ] Los 12 recuerdos
- [ ] Las 110 cartas
- [ ] Los 4 minijuegos en difícil
- [ ] Los 15 retos
- [ ] El Cristal en brillo 10
