# Estado del proyecto — El Legado de la Última Guerra

**Actualizado:** 27 de octubre de 2026
**Fase:** 1 — preparación y auditoría
**Estado general:** servidor inicia, completa migraciones y conserva los datos tras reinicio en carpeta temporal; cliente, servidor y editor funcionan juntos en la prueba local. La cuenta y el personaje persisten tras reiniciar el servidor; el mapa guardado se ve en el cliente con terreno, zonas de piedra y sprite.

## Base de trabajo

- Repositorio: `Intersect-Engine-v0.8.0-beta.582`.
- Rama activa: `ultima-guerra`.
- Versión base: `v0.8.0-beta.582` (beta), commit `875178d5a48a372a15340bc4ca9965a56c389e0b`.
- `origin` todavía apunta a `https://github.com/AscensionGameDev/Intersect-Engine.git`; no se configuró ni publicó un fork personal.
- Submódulos descargados: LiteNetLib `e5cd7ea0069f57cf3ba99e638139499cc22442a8`; DockPanel Suite `a373cf7fbaa0a79c4790ac5cb26eba48944a58bc`.
- .NET SDK disponible: 8.0.427 y 10.0.401. Git for Windows: 2.56.0.windows.4.

## Cambios del proyecto

- La eliminación de `AGENTS.md` quedó registrada en el commit local `8e6cc353a2b10fe44c9e3a15c4571fa103c22e88` (`Remove AGENTS.md`).
- `Intersect.Server/Intersect.Server.csproj`: ImageSharp actualizado de 3.1.11 a 3.2.0. El cambio aún requiere revisión/commit.
- `Intersect.Server.Core/Intersect.Server.Core.csproj`: `SQLitePCLRaw.lib.e_sqlite3` actualizado de 2.1.10 a 2.1.14; `bundle_green` se conserva en 2.1.10. Restauración y compilación Debug con esos assets: correctas.
- Se corrigieron las expresiones SQL de las migraciones SQLite `20221016001123_MultipleBonusEffectsMigration` y `20221119214350_ItemProperties` para usar literales de texto entre comillas simples. En la segunda migración se corrigieron las cuatro tablas de objetos afectadas. Las consultas MySQL y las mecánicas quedaron intactas.

## Compilación y calidad

- Compilación Debug inicial de `Intersect.sln`: correcta en 27,1 s con 2.179 advertencias.
- Tras restaurar SQLitePCLRaw, nueva compilación Debug: **correcta**, en 13,9 s con **79 advertencias** y sin errores. Al ser incremental, el total no se compara directamente con el build inicial.
- La salida de compilación indica que ImageSharp 3.2.0 permite Debug sin clave, pero Release requiere una clave válida.
- La alerta GHSA-2m69-gcr7-jv3q afecta `lib.e_sqlite3` hasta 2.1.11; los assets usan 2.1.14. La DLL nativa SQLite del build es 3.54.0 y reporta `DQS=0`. Se corrigió la consulta de `20221016001123_MultipleBonusEffectsMigration` para delimitar literales con comillas simples. La consulta corregida pasó una ejecución en memoria con la misma DLL.
- La primera restauración falló al pedir `SQLitePCLRaw.bundle_green` 2.1.14, versión no publicada; tras dejarla en 2.1.10, la restauración y compilación terminaron correctamente. Restore dejó dos advertencias: una alerta moderada para `NCalcSync` 3.8.0 y compatibilidad `NU1701` de `Mono.Data.Sqlite.Portable` 1.0.3.5 con el editor.
- Compilación posterior a la corrección: `Intersect.Server.Core` compiló con 65 advertencias y 0 errores; `Intersect.Server` compiló con 7 advertencias y 0 errores (build con referencias de proyecto ya generadas). Un build completo de la solución en el entorno de Codex no pudo ejecutar `KeyGenerationTask` de `Intersect.Network` (MSB4216, host de tareas MSBuild no disponible); esto es independiente del cambio SQL.
- Verificación de ejecución: en la carpeta temporal `Intersect-smoke-fix2-...`, el servidor completó migraciones, creó una clase predeterminada y un mapa, llegó a `Starting main thread` y escuchó UDP 5400. Tras cerrar y reiniciar con la misma carpeta, conservó clase, mapa, cuenta y personaje; persistencia SQLite confirmada según la verificación del usuario. El cliente conectó y fue aprobado por `127.0.0.1:5400`. El usuario abrió el editor y guardó un mapa simple; la captura posterior confirma que el cliente muestra el terreno, las zonas de piedra y el sprite. No se probó acceso externo. El build completo de la solución en Codex sigue bloqueado por MSB4216 al iniciar `KeyGenerationTask`.
- Avisos no bloqueantes durante el arranque: carpeta opcional `resources\plugins` ausente; UPnP no disponible; el certificado HTTPS carece de extensión SAN. Revisar el certificado antes de usar HTTPS en un navegador; ninguno de estos avisos impidió que el servidor local arrancara.
- Pruebas del servidor: el usuario reportó `Intersect.Tests.Server` con **44 correctas, 0 errores y 0 omitidas** en 4,3 s; compilación Debug correcta con 5 advertencias en 10,6 s. No se recibió el detalle de esas advertencias, por lo que queda pendiente confirmar si entre ellas aparece el aviso de licencia de ImageSharp.
- Flujo local cliente-servidor-editor: **confirmado**. Creación y persistencia de cuenta/personaje, guardado del mapa y visualización de terreno, zonas de piedra y sprite en el cliente: **confirmados por el usuario y la captura**.

## Hallazgos ya comprobados

- Intersect tiene cinco estadísticas propias: Attack, AbilityPower, Defense, MagicResist y Speed. No son equivalentes a los cinco atributos acordados para Última Guerra.
- Ya existen combate resuelto por servidor, bloqueo, proyectiles, estados, animaciones de ataque, paperdoll configurable, autotiles XP, pathfinding A*, crafting, quests, eventos, grupos, guilds y comercio.
- La persistencia admite SQLite y MySQL mediante contextos/migraciones. Los arrays de estadísticas guardados dependen del enum, por lo que la futura sustitución requiere migración.
- El editor es WinForms/DirectX y se ejecuta en Windows.

## Próximos pasos

1. Configurar una licencia válida de ImageSharp para las compilaciones del servidor o evaluar una alternativa.
2. Confirmar en el detalle de compilación que no aparece el aviso de licencia de ImageSharp; antes de Release, verificar también la generación de claves de red en esa configuración.
3. Preparar el remoto personal del fork, conservando `origin` como upstream, antes de cualquier cambio de mecánicas.
4. Después de validar la base, diseñar el contrato de los cinco atributos y su migración, sin implementarlos hasta autorización.

## Decisiones confirmadas

- El nombre oficial del juego es **El Legado de la Última Guerra**; la carpeta local se llama `ultima guerra`.
- Se usarán sprites de proporciones RPG Maker XP al inicio.
- Las animaciones de combate con brazos, armas y equipo en capas quedan como objetivo posterior.
- La prioridad inicial es motor, mecánicas y herramientas.
