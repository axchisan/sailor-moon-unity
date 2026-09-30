# Controles

Todos pasan por `InputReader` (`Scripts/Runtime/Player/InputReader.cs`): el
resto del juego no sabe con qué se está jugando.

## PC / Mac (teclado y ratón)

| Acción | Tecla |
|---|---|
| Mover | WASD o flechas |
| Andar en vez de correr | Shift (mantener) |
| Mirar | Ratón. **Clic en el juego para capturarlo**, Esc para soltarlo. Sin capturar: arrastrar con el botón derecho |
| Acercar / alejar la cámara | Rueda |
| Saltar | Espacio |
| Atacar | J o clic izquierdo |
| Especial (cuando la barra brilla) | K |
| Volver al inicio | R |
| Panel de rendimiento | F1 |
| Cambiar de escena (desarrollo) | F2 |

## Mando

| Acción | Botón |
|---|---|
| Mover / mirar | Stick izquierdo / derecho |
| Saltar | A (Xbox) · ✕ (PlayStation) |
| Atacar | X (Xbox) · □ (PlayStation) |
| Especial | Y (Xbox) · △ (PlayStation) |
| Volver al inicio | Select / View |

## Móvil

Joystick flotante en la mitad izquierda (aparece donde se pone el dedo),
arrastrar en la mitad derecha para mirar, botones de saltar y atacar abajo a la
derecha, y el del especial (amarillo) encima. Tres dedos a la vez: panel de
rendimiento. Cuatro dedos: cambiar de escena (desarrollo). En PC los botones táctiles
no se muestran (`TouchControls._showOnDesktop` para verlos en el editor).

## Jugar en el Mac

1. Abrir el proyecto en Unity y la escena `Assets/_Project/Scenes/Sandbox/PerfTest.unity`.
2. **Play** (▶ arriba en el centro, o Cmd+P).
3. Clic dentro de la vista *Game* para capturar el ratón.

Consejo: en la vista *Game*, *Play Maximized* (en el desplegable de arriba a la
derecha) para jugar a pantalla completa del editor.
