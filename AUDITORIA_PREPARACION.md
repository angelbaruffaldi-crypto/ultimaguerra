# Auditoría de preparación — estadísticas y sprites

**Fecha:** 28 de octubre de 2026  
**Alcance:** lectura del código y registro de decisiones. No se modificaron código, configuración, recursos ni mecánicas.

## Estado verificado

- Rama local `ultima-guerra`; seguimiento configurado a `origin/ultima-guerra`. El usuario informó que el envío a GitHub terminó correctamente.
- Último resultado de pruebas comunicado por el usuario: `Intersect.Tests.Server`, 44 correctas, 0 errores, 0 omitidas; Debug correcto con 343 advertencias en 31,1 s.
- La prueba local de cliente, servidor y editor, la persistencia de cuenta/personaje y un mapa simple ya fueron confirmados anteriormente.
- No se encontraron en esta copia hojas de personaje/equipo del generador Universal LPC. Los PNG encontrados pertenecen a recursos técnicos del motor/editor/vendor o a ejemplos.

## Flujo actual de estadísticas

1. `Framework/Intersect.Framework.Core/Entities/Stat.cs` define cinco índices existentes: `Attack`, `AbilityPower`, `Defense`, `MagicResist` y `Speed`.
2. El servidor crea y calcula cada valor en `Intersect.Server.Core/Entities/Combat/Stat.cs`. El cálculo suma valor base, puntos asignados, modificadores de objetos y efectos de hechizos.
3. `Intersect.Server.Core/Entities/Entity.cs` guarda `BaseStats` y `StatPointAllocations` como arreglos serializados en columnas de base de datos. Su longitud depende de `Enum.GetValues<Stat>().Length`; el orden del enum determina la posición de cada valor.
4. Las clases y NPC aportan estadísticas iniciales; el jugador puede gastar puntos. El editor presenta esos valores y el protocolo comparte las actualizaciones entre cliente y servidor.

**Consecuencia:** reemplazar nombres conservando cinco entradas no garantiza compatibilidad. Antes de programar hay que decidir el significado de cada atributo y revisar fórmulas, objetos, hechizos, clases, NPC, editor, condiciones/eventos, paquetes y datos persistidos. Los datos de prueba no son una base de producción, pero toda migración futura debe contemplar SQLite y MySQL.

## Flujo actual de sprites y paperdoll

- `Intersect.Client.Core/Entities/Entity.cs` escoge el número de fotogramas según la acción (`Normal`, `Idle`, `Attack`, `Shoot`, `Cast` o `Weapon`). El recorte del fotograma usa una cuadrícula: ancho dividido por fotogramas y alto dividido por la cantidad configurada de direcciones.
- El renderizador busca paperdolls específicos por acción y admite un archivo predeterminado. También obtiene el orden de capas/direcciones de `Options.Instance.Equipment.Paperdoll.Directions`.
- `Intersect.Server.Core/Networking/PacketSender.cs` envía los paperdolls equipados según el orden configurado. El editor lista recursos de la carpeta `paperdolls` y permite asignarlos al equipo.

**Consecuencia:** el paperdoll y el cuerpo ya se dibujan por capas, pero la lectura actual espera que las imágenes correspondan a la cuadrícula y a las acciones configuradas por Intersect. Una hoja grande con varias acciones distribuidas en otro diseño no se interpreta automáticamente solo porque cuerpo, armadura y arma estén alineados entre sí. Hay que verificar el mapa de filas/columnas, direcciones, fotogramas por acción, pivote y orden de capas con archivos reales.

## Referencia de arte compartida en el chat

En la conversación **“Adaptar Sprites A Intersect”** se describieron PNG del generador Universal LPC: cuerpo y armadura de 493 × 2048, y espada de 682 × 2048. El usuario indicó que el generador coloca las capas alineadas. Esos archivos no están dentro de este repositorio, así que estas medidas y la alineación todavía no se comprobaron de nuevo en la carpeta de trabajo.

La dirección visual queda en evaluación: el motor se probó primero con sprites estándar estilo RPG Maker XP; el siguiente ensayo de arte puede usar una muestra Universal LPC con cuerpo, una pieza de armadura y un arma. Se deben conservar los originales y revisar también sus condiciones de uso antes de distribuir el juego.

## Próxima secuencia segura

1. Copiar una muestra pequeña del generador a una carpeta de pruebas del proyecto, conservando los PNG originales y anotando qué representa cada archivo.
2. Medir dimensiones y localizar las celdas de quieto, caminar y ataque por dirección; superponer cuerpo, armadura y arma para verificar alineación y transparencia.
3. Comparar ese mapa con los recortes y nombres de recursos que usa Intersect. Registrar qué encaja sin cambios y qué exige un formato/controlador adicional.
4. Revisar y acordar el diseño antes de modificar el motor.
5. En una etapa separada, completar el mapa de datos de atributos y acordar sus reglas. No deducir fórmulas ni efectos a partir de los nombres.

Hasta completar esas comprobaciones, no implementar nuevos atributos ni cambiar el renderizador o los recursos del juego.
