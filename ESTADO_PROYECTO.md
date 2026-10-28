# Estado del proyecto — El Legado de la Última Guerra

**Actualizado:** 28 de octubre de 2026
**Fase:** 2 — diseño de mecánicas y contratos
**Estado general:** servidor inicia, completa migraciones y conserva los datos tras reinicio en carpeta temporal; cliente, servidor y editor funcionan juntos en la prueba local. La cuenta y el personaje persisten tras reiniciar el servidor; el mapa guardado se ve en el cliente con terreno, zonas de piedra y sprite.

## Base de trabajo

- Repositorio: `Intersect-Engine-v0.8.0-beta.582`.
- Rama activa: `ultima-guerra`.
- Versión base: `v0.8.0-beta.582` (beta), commit `875178d5a48a372a15340bc4ca9965a56c389e0b`.
- `origin` apunta al repositorio personal `https://github.com/angelbaruffaldi-crypto/ultimaguerra.git`; `upstream` conserva el repositorio oficial. El usuario confirmó la publicación de `ultima-guerra`; localmente la rama sigue a `origin/ultima-guerra`.
- Punto de control de código previo a esta actualización: commit `50097cceb` (`Record local sprite inventory and stats priority`), en la rama `ultima-guerra`; estaba sincronizado con `origin`.
- Submódulos descargados: LiteNetLib `e5cd7ea0069f57cf3ba99e638139499cc22442a8`; DockPanel Suite `a373cf7fbaa0a79c4790ac5cb26eba48944a58bc`.
- .NET SDK disponible: 8.0.427 y 10.0.401. Git for Windows: 2.56.0.windows.4.

## Cambios del proyecto

- La eliminación de `AGENTS.md` y las actualizaciones de dependencias quedaron registradas en la historia de la rama; no son cambios pendientes.
- `Intersect.Server/Intersect.Server.csproj`: ImageSharp actualizado de 3.1.11 a 3.2.0.
- `Intersect.Server.Core/Intersect.Server.Core.csproj`: `SQLitePCLRaw.lib.e_sqlite3` actualizado de 2.1.10 a 2.1.14; `bundle_green` se conserva en 2.1.10. Restauración y compilación Debug con esos assets: correctas.
- Se corrigieron las expresiones SQL de las migraciones SQLite `20221016001123_MultipleBonusEffectsMigration` y `20221119214350_ItemProperties` para usar literales de texto entre comillas simples. En la segunda migración se corrigieron las cuatro tablas de objetos afectadas. Las consultas MySQL y las mecánicas quedaron intactas.

## Compilación y calidad

- Compilación Debug inicial de `Intersect.sln`: correcta en 27,1 s con 2.179 advertencias.
- Tras restaurar SQLitePCLRaw, nueva compilación Debug: **correcta**, en 13,9 s con **79 advertencias** y sin errores. Al ser incremental, el total no se compara directamente con el build inicial.
- La salida de compilación indica que ImageSharp 3.2.0 permite Debug sin clave, pero Release requiere una clave válida.
- La alerta GHSA-2m69-gcr7-jv3q afecta `lib.e_sqlite3` hasta 2.1.11; los assets usan 2.1.14. La DLL nativa SQLite del build es 3.54.0 y reporta `DQS=0`. Se corrigió la consulta de `20221016001123_MultipleBonusEffectsMigration` para delimitar literales con comillas simples. La consulta corregida pasó una ejecución en memoria con la misma DLL.
- La primera restauración falló al pedir `SQLitePCLRaw.bundle_green` 2.1.14, versión no publicada; tras dejarla en 2.1.10, la restauración y compilación terminaron correctamente. Restore dejó dos advertencias: una alerta moderada para `NCalcSync` 3.8.0 y compatibilidad `NU1701` de `Mono.Data.Sqlite.Portable` 1.0.3.5 con el editor.
- Compilación posterior a la corrección: `Intersect.Server.Core` compiló con 65 advertencias y 0 errores; `Intersect.Server` compiló con 7 advertencias y 0 errores (build con referencias de proyecto ya generadas). Un build completo de la solución en el entorno de Codex no pudo ejecutar `KeyGenerationTask` de `Intersect.Network` (MSB4216, host de tareas MSBuild no disponible); esto es independiente del cambio SQL. Un intento separado de compilar `Intersect.Network` en Release también salió con código 1 sin resumen de error útil y no generó `bin\Release\keys\network.handshake.bkey.pub`; las pruebas Debug sí pasaron.
- Verificación de ejecución: en la carpeta temporal `Intersect-smoke-fix2-...`, el servidor completó migraciones, creó una clase predeterminada y un mapa, llegó a `Starting main thread` y escuchó UDP 5400. Tras cerrar y reiniciar con la misma carpeta, conservó clase, mapa, cuenta y personaje; persistencia SQLite confirmada según la verificación del usuario. El cliente conectó y fue aprobado por `127.0.0.1:5400`. El usuario abrió el editor y guardó un mapa simple; la captura posterior confirma que el cliente muestra el terreno, las zonas de piedra y el sprite. No se probó acceso externo. El build completo de la solución en Codex sigue bloqueado por MSB4216 al iniciar `KeyGenerationTask`.
- Avisos no bloqueantes durante el arranque: carpeta opcional `resources\plugins` ausente; UPnP no disponible; el certificado HTTPS carece de extensión SAN. Revisar el certificado antes de usar HTTPS en un navegador; ninguno de estos avisos impidió que el servidor local arrancara.
- Pruebas del servidor: resultado más reciente aportado por el usuario para `Intersect.Tests.Server`: **44 correctas, 0 errores y 0 omitidas** en 3,8 s; compilación Debug correcta con 343 advertencias en 31,1 s. No se recibió el detalle de esas advertencias, así que no se puede confirmar desde ese resumen si aparece un aviso de licencia de ImageSharp.
- Flujo local cliente-servidor-editor: **confirmado**. Creación y persistencia de cuenta/personaje, guardado del mapa y visualización de terreno, zonas de piedra y sprite en el cliente: **confirmados por el usuario y la captura**.

## Hallazgos ya comprobados

- Intersect tiene cinco estadísticas propias: Attack, AbilityPower, Defense, MagicResist y Speed. No son equivalentes a los cinco atributos acordados para Última Guerra.
- Ya existen combate resuelto por servidor, bloqueo, proyectiles, estados, animaciones de ataque, paperdoll configurable, autotiles XP, pathfinding A*, crafting, quests, eventos, grupos, guilds y comercio.
- La persistencia admite SQLite y MySQL mediante contextos/migraciones. Los arrays de estadísticas guardados dependen del enum, por lo que la futura sustitución requiere migración.
- El editor es WinForms/DirectX y se ejecuta en Windows.

## Próximos pasos

La secuencia completa y el estado de cada tarea están en `HOJA_DE_RUTA.md`. El mapeo transversal inicial de estadísticas está completado; el siguiente entregable es la propuesta de reglas y progresión para los cinco atributos. No se han cambiado sus reglas.

## Decisiones confirmadas

- El nombre oficial del juego es **El Legado de la Última Guerra**; la carpeta local se llama `ultima guerra`.
- La primera prueba técnica del motor usó sprites estándar y el flujo cliente/editor ya se comprobó.
- Los sprites LPC para cuerpo, armadura y armas ya están en `Client and Editor/resources/entities` y `Client and Editor/resources/paperdolls`. Sus dimensiones se registraron en `AUDITORIA_PREPARACION.md`; la correspondencia de celdas y alineación todavía requiere prueba técnica.
- Las animaciones de combate con brazos, armas y equipo en capas quedan como objetivo posterior, sujeto a validar las hojas y el formato de animación.
- La prioridad inicial es motor, mecánicas y herramientas.
