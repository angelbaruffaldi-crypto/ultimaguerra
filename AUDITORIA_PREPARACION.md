# Auditoría de preparación — estadísticas y sprites

**Fecha:** 28 de octubre de 2026  
**Alcance:** lectura del código y registro de decisiones. No se modificaron código, configuración, recursos ni mecánicas.

## Estado verificado

- Rama local `ultima-guerra`; seguimiento configurado a `origin/ultima-guerra`. El usuario informó que el envío a GitHub terminó correctamente.
- Último resultado de pruebas comunicado por el usuario: `Intersect.Tests.Server`, 44 correctas, 0 errores, 0 omitidas; Debug correcto con 343 advertencias en 31,1 s.
- La prueba local de cliente, servidor y editor, la persistencia de cuenta/personaje y un mapa simple ya fueron confirmados anteriormente.
- Los ocho archivos solicitados ya están en las carpetas indicadas dentro de `Client and Editor/resources`; no fue necesario moverlos ni duplicarlos.
- `Client and Editor` no es un repositorio Git. Estos recursos están guardados localmente, pero aún no forman parte de la rama publicada del código fuente.

| Carpeta | Archivo | Dimensiones |
|---|---|---:|
| `entities` | `masculino.png` | 832 × 3456 |
| `entities` | `femenino.png` | 832 × 3456 |
| `paperdolls` | `arco.png` | 1152 × 3968 |
| `paperdolls` | `sable.png` | 1152 × 4224 |
| `paperdolls` | `lanza.png` | 1536 × 4736 |
| `paperdolls` | `pantalones cuero.png` | 832 × 3456 |
| `paperdolls` | `baston.png` | 1536 × 4224 |
| `paperdolls` | `pechera cuero.png` | 832 × 3456 |

## Flujo actual de estadísticas

1. `Framework/Intersect.Framework.Core/Entities/Stat.cs` define cinco índices existentes: `Attack`, `AbilityPower`, `Defense`, `MagicResist` y `Speed`.
2. El servidor crea y calcula cada valor en `Intersect.Server.Core/Entities/Combat/Stat.cs`. El cálculo suma valor base, puntos asignados, modificadores de objetos y efectos de hechizos.
3. `Intersect.Server.Core/Entities/Entity.cs` guarda `BaseStats` y `StatPointAllocations` como arreglos serializados en columnas de base de datos. Su longitud depende de `Enum.GetValues<Stat>().Length`; el orden del enum determina la posición de cada valor.
4. Las clases y NPC aportan estadísticas iniciales; el jugador puede gastar puntos. El editor presenta esos valores y el protocolo comparte las actualizaciones entre cliente y servidor.

**Consecuencia:** reemplazar nombres conservando cinco entradas no garantiza compatibilidad. Antes de programar hay que decidir el significado de cada atributo y revisar fórmulas, objetos, hechizos, clases, NPC, editor, condiciones/eventos, paquetes y datos persistidos. Los datos de prueba no son una base de producción, pero toda migración futura debe contemplar SQLite y MySQL.

### Alcance transversal localizado

- `Intersect.Server.Core/General/Formulas.cs` expone las cinco estadísticas actuales mediante nombres de fórmula `A_*` y `V_*`; esas expresiones pueden estar guardadas en contenido del juego.
- La estadística `Speed` también afecta el intervalo de movimiento en `Intersect.Server.Core/Entities/Entity.cs` y `Intersect.Client.Core/Entities/Entity.cs`.
- Las clases y NPC guardan valores por índice; el editor los lee/escribe campo por campo. Los hechizos y objetos tienen arreglos de cambios de estadísticas, y el equipo tiene rangos aleatorios por estadística.
- El cliente muestra y permite asignar puntos desde la ventana del personaje; el servidor valida esas asignaciones. Hay además condiciones/eventos que consultan estadísticas.
- `Intersect.Server.Core/General/Formulas.cs` pasa las estadísticas actuales a las fórmulas de combate; `Intersect.Editor/Forms/Editors/frmSpell.cs` edita cambios de estadísticas de hechizos; `frmClass.cs` y `frmNpc.cs` exponen valores y progresión en el editor.
- `Intersect.Client.Core/Interface/Game/Character/CharacterWindow.cs` muestra/asigna puntos y envía solicitudes al servidor. Los nombres aparecen en localización y en la vista del editor.

Esto confirma que la migración afecta más que los nombres visibles: combate, movimiento, objetos, progresión, editor, interfaz, eventos y fórmulas. El primer entregable mecánico debe ser una tabla acordada de significado y uso para cada atributo antes de tocar código o datos.

## Flujo actual de sprites y paperdoll

- `Intersect.Client.Core/Entities/Entity.cs` escoge el número de fotogramas según la acción (`Normal`, `Idle`, `Attack`, `Shoot`, `Cast` o `Weapon`). El recorte del fotograma usa una cuadrícula: ancho dividido por fotogramas y alto dividido por la cantidad configurada de direcciones.
- El renderizador busca paperdolls específicos por acción y admite un archivo predeterminado. También obtiene el orden de capas/direcciones de `Options.Instance.Equipment.Paperdoll.Directions`.
- `Intersect.Server.Core/Networking/PacketSender.cs` envía los paperdolls equipados según el orden configurado. El editor lista recursos de la carpeta `paperdolls` y permite asignarlos al equipo.

**Consecuencia:** el paperdoll y el cuerpo ya se dibujan por capas, pero la lectura actual espera que las imágenes correspondan a la cuadrícula y a las acciones configuradas por Intersect. Una hoja grande con varias acciones distribuidas en otro diseño no se interpreta automáticamente solo porque cuerpo, armadura y arma estén alineados entre sí. Hay que verificar el mapa de filas/columnas, direcciones, fotogramas por acción, pivote y orden de capas con archivos reales.

## Referencia de arte compartida en el chat

En la conversación **“Adaptar Sprites A Intersect”** se describieron otros PNG del generador Universal LPC: cuerpo y armadura de 493 × 2048, y espada de 682 × 2048. Los ocho archivos de la tabla anterior son los que están hoy en las carpetas pedidas y tienen dimensiones distintas. El usuario indicó que el generador alinea las capas; la inspección visual de las hojas actuales confirma que comparten un formato de hoja extensa, pero sus dimensiones varían entre cuerpo, ropa y armas. La correspondencia exacta de celdas debe probarse antes de asumir sincronización.

La dirección visual queda en evaluación: el motor se probó primero con sprites estándar estilo RPG Maker XP; los recursos LPC ya están guardados en el paquete local cliente/editor. Se deben conservar los originales y revisar sus condiciones de uso antes de distribuir el juego.

## Próxima secuencia segura

1. Priorizar la auditoría de atributos y mecánicas: completar el inventario de lugares donde se leen y escriben sus índices y fórmulas.
2. Acordar el significado y los usos de Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría antes de implementar; no inferir efectos solo por los nombres.
3. Después, usar los recursos ya guardados para medir celdas por acción/dirección y validar la alineación de cuerpo, armadura y armas en una prueba separada.
4. Revisar y acordar cualquier extensión del renderizador antes de modificarlo. Mantener los PNG originales y registrar licencias antes de distribución.

Hasta completar esas comprobaciones, no implementar nuevos atributos ni cambiar el renderizador o los recursos del juego.
