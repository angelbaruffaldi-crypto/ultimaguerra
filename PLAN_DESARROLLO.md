# El Legado de la Última Guerra — Fase 1: preparación, auditoría y compilación

**Fecha:** 27 de octubre de 2026
**Estado:** auditoría estática, compilación, arranque, migraciones, persistencia tras reinicio y flujo cliente-servidor-editor confirmados en carpetas temporales. El usuario guardó un mapa simple y la captura confirma que el cliente muestra el terreno, zonas de piedra y sprite.

## 1. Resumen ejecutivo

Última Guerra ya tiene una copia Git funcional del código fuente de Intersect, con los dos submódulos requeridos. La referencia elegida es `v0.8.0-beta.582`, commit `875178d5a48a372a15340bc4ca9965a56c389e0b`. Esta etiqueta coincide con los ejecutables que ya estaban en la carpeta `ultima guerra`.

La etiqueta es **beta**, no una versión estable. Se conserva como línea de base para que el código corresponda a los ejecutables existentes. Antes de usarla como base de producción, habrá que decidir si se continúa con esta beta o se migra a una versión estable disponible.

La compilación inicial de la solución completa con .NET 8.0.427 fue correcta en 27,1 segundos, con 2.179 advertencias. Tras restaurar SQLitePCLRaw, una compilación Debug incremental terminó correctamente en 13,9 segundos, con 79 advertencias y sin errores. Esto confirma que la actualización compila; no demuestra todavía que SQLite conserve datos ni que cliente y servidor puedan conectarse.

No se cambiaron mecánicas. En la copia de trabajo hay dos cambios de dependencias: `SixLabors.ImageSharp` 3.1.11 → 3.2.0 en `Intersect.Server/Intersect.Server.csproj`, y la biblioteca nativa `SQLitePCLRaw.lib.e_sqlite3` 2.1.10 → 2.1.14 en `Intersect.Server.Core/Intersect.Server.Core.csproj`. Se mantiene `SQLitePCLRaw.bundle_green` en 2.1.10 porque NuGet no ofrece 2.1.14 para ese paquete. El registro del usuario confirma que ImageSharp 3.2.0 permite continuar Debug sin clave y que Release requiere una clave válida. `lib.e_sqlite3` 2.1.14 queda fuera del rango afectado por GHSA-2m69-gcr7-jv3q (hasta 2.1.11); la restauración y compilación Debug posteriores fueron correctas. La persistencia de la cuenta y el personaje se confirmó después de reiniciar el servidor.

## 2. Estado de Git y herramientas

- **Repositorio:** `Intersect-Engine-v0.8.0-beta.582` dentro de `C:\Users\ANGEL\Desktop\ultima guerra`.
- **Rama activa:** `ultima-guerra`.
- **Punto de partida:** etiqueta `v0.8.0-beta.582`, commit `875178d5a48a372a15340bc4ca9965a56c389e0b`.
- **Commit local posterior:** `8e6cc353a2b10fe44c9e3a15c4571fa103c22e88` (`Remove AGENTS.md`).
- **Remoto actual:** `origin` apunta al repositorio oficial `AscensionGameDev/Intersect-Engine`; todavía no hay remoto de un fork personal. No se hizo `push`.
- **Submódulos confirmados al clonar:** LiteNetLib `e5cd7ea0069f57cf3ba99e638139499cc22442a8`; DockPanel Suite `a373cf7fbaa0a79c4790ac5cb26eba48944a58bc`.
- **SDK instalado:** .NET 8.0.427 y 10.0.401. El motor documenta .NET 8 para desarrollo.
- **Git instalado:** Git for Windows 2.56.0.windows.4.
- **Estado de trabajo al redactar:** `Intersect.Server/Intersect.Server.csproj` tiene un cambio pendiente por ImageSharp 3.2.0 y `Intersect.Server.Core/Intersect.Server.Core.csproj` por `SQLitePCLRaw.lib.e_sqlite3` 2.1.14. No hay cambios de mecánicas.
- El ZIP `Intersect-Engine-main` y los ejecutables originales permanecen fuera de esta copia Git, en la carpeta contenedora.

El tag oficial se conserva como referencia limpia dentro de Git. La rama de Última Guerra y el tag deben mantenerse separados para poder comparar futuras modificaciones con la base original.

## 3. Arquitectura encontrada

| Área | Responsabilidad principal | Módulos de referencia |
|---|---|---|
| Tipos y datos compartidos | Descriptores de contenido, configuración, enums, mensajes de red y contratos comunes | `Framework/Intersect.Framework.Core` |
| Transporte | Conexiones, paquetes y capa LiteNetLib | `Intersect.Network`, `vendor/LiteNetLib` |
| Cliente | Inicio y composición del cliente | `Intersect.Client` |
| Cliente y presentación | Entidades, mapas, interfaz, animaciones y recepción de paquetes | `Intersect.Client.Core`, `Intersect.Client.Framework` |
| Servidor | Inicio, configuración, sitio web, conexión y base de datos | `Intersect.Server` |
| Lógica del juego en servidor | Jugadores, entidades, combate, NPC, mapas, eventos y persistencia | `Intersect.Server.Core`, `Intersect.Server.Framework` |
| Editor | Formularios WinForms, mapas y edición de objetos del juego | `Intersect.Editor` |
| Modo de un jugador | Implementación de red local para jugar sin servidor remoto | `Intersect.SinglePlayer` |
| Pruebas | Pruebas unitarias de utilidades y parte de progresión del jugador | `Intersect.Tests`, `Intersect.Tests.Server`; varios proyectos de prueba de cliente/editor/red son actualmente stubs |

El editor usa Windows Forms/DirectX y debe ejecutarse en Windows. El cliente y el servidor tienen soporte multiplataforma documentado, sujeto a los requisitos de `README.md` y `REQUIREMENTS.md`.

## 4. Persistencia y datos

El servidor admite **SQLite** y **MySQL**. SQLite es el valor predeterminado en `Intersect.Server/Program.cs`; MySQL se habilita mediante `DatabaseType` y `ConnectionString`.

La persistencia se divide en contextos de juego, jugadores y registros, con implementaciones SQLite y MySQL y migraciones separadas:

- `Intersect.Server.Core/Database/GameData`
- `Intersect.Server.Core/Database/PlayerData`
- `Intersect.Server.Core/Database/Logging`
- `Intersect.Server/Database/GameData`
- `Intersect.Server/Database/PlayerData`
- `Intersect.Server/Migrations` y `Intersect.Server.Core/Migrations`
- `Intersect.Server.Core/Entities/Player.Database.cs`

Los valores de atributos del jugador se almacenan en arrays (`BaseStats` y `StatPointAllocations`) cuyo tamaño depende del enum de atributos. Cambiar cantidad u orden de atributos exige revisar serialización, migraciones, contenido, cliente, editor y fórmulas; no alcanza con cambiar las etiquetas visibles.

La creación de cuenta y personaje se probó de extremo a extremo según confirmó el usuario. También verificó que la cuenta y el personaje seguían disponibles tras reiniciar el servidor con la misma base de datos.

## 5. Auditoría de sistemas

### 5.1 Estadísticas y progresión

- **Existe:** nivel, experiencia, puntos asignables y modificadores de atributos base, equipo y hechizos. El enum actual es `Attack`, `AbilityPower`, `Defense`, `MagicResist` y `Speed`.
- **Módulos:** `Framework/Intersect.Framework.Core/Entities/Stat.cs`, `Intersect.Server.Core/Entities/Combat/Stat.cs`, `Intersect.Server.Core/Entities/Player.cs`, `Intersect.Server.Core/General/Formulas.cs`, `Framework/Intersect.Framework.Core/GameObjects/PlayerClass/ClassDescriptor.cs`, `Intersect.Server.Core/Database` y formularios de clase/objetos del editor.
- **Diferencia con El Legado de la Última Guerra:** los cinco atributos confirmados (`Fuerza`, `Destreza`, `Inteligencia`, `Agilidad`, `Sabiduría`) sustituyen semánticas distintas, aunque también sean cinco.
- **Recomendación:** antes de programar, definir el contrato de datos y una tabla atributo → fórmulas, progreso, equipo, habilidades y visualización. Luego diseñar migración para los arrays guardados.
- **Dificultad/riesgo:** alta; cambio transversal con riesgo de leer datos antiguos en posiciones equivocadas o romper contenido existente.

### 5.2 Combate y animaciones

- **Existe:** ataques, tiempos de ataque, daño físico/mágico/verdadero, fórmulas configurables, bloqueo, efectos de estado, ataques por habilidad y proyectiles. El servidor recibe intenciones como `AttackPacket` y valida estado, alcance y objetivo antes de resolver daño.
- **Módulos:** `Intersect.Server.Core/Entities/Entity.cs`, `Intersect.Server.Core/Entities/Player.cs`, `Intersect.Server.Core/Networking/PacketHandler.cs`, `Intersect.Server.Core/General/Formulas.cs`, `Intersect.Server.Core/Networking/PacketSender.cs`, descriptores de armas, hechizos y proyectiles en `Framework/Intersect.Framework.Core/GameObjects`.
- **Animación disponible:** estados de sprite `Normal`, `Idle`, `Attack`, `Shoot`, `Cast` y `Weapon`; los objetos permiten animación de ataque/equipo y los proyectiles tienen descriptores propios.
- **Límite para el diseño confirmado:** hay piezas para ataques y animaciones, pero la auditoría no confirmó una secuencia de combate por fotograma que sincronice capas separadas de brazo, arma y cuerpo al estilo propuesto.
- **Recomendación:** reutilizar la resolución autoritativa del servidor y evaluar por separado el formato de animación y sincronización antes de introducir capas nuevas.
- **Dificultad/riesgo:** alta; cliente, servidor, paquetes y contenido de editor deben compartir tiempos y estados.

### 5.3 Personajes y paperdoll

- **Existe:** sprites de entidad, equipo visual, paperdoll configurable por orden de capas y sexo, y composición de capas para mostrar equipamiento.
- **Módulos:** `Framework/Intersect.Framework.Core/GameObjects/Items/ItemDescriptor.cs`, `Framework/Intersect.Framework.Core/Config/EquipmentOptions.cs`, `Intersect.Server.Core/Networking/PacketSender.cs`, `Intersect.Client.Core/Entities/Entity.cs`, `Intersect.Client.Core/Interface/Menu/SelectCharacterWindow.cs`.
- **Recomendación:** primero validar los sprites XP elegidos con los tamaños/direcciones/fotogramas que espera Intersect. Mantener el paperdoll existente como base; extender el formato solo si las armas o brazos no pueden expresarse con sus capas actuales.
- **Dificultad/riesgo:** media-alta; los assets, las opciones del servidor y la representación del cliente deben coincidir.

### 5.4 Mapas, autotiles y biomas

- **Existe:** capas de mapa, atributos de colisión/sonido/animación/recursos, herramientas de pintura, selección, borrado y relleno, y autotiles normal, animado, acantilado y formato XP.
- **Módulos:** `Framework/Intersect.Framework.Core/GameObjects/Maps/MapAutotiles.cs`, `Framework/Intersect.Framework.Core/GameObjects/Maps/Tile.cs`, `Intersect.Editor/Maps/MapInstance.cs`, `Intersect.Editor/Forms/DockingElements/frmMapEditor.cs`, `Intersect.Editor/Core/Graphics.cs`.
- **Límite observado:** hay autotiling XP, pero no se identificó en esta inspección un sistema de biomas, pinceles de distribución por reglas ni generación procedural de mapas.
- **Recomendación:** conservar el editor actual y planificar mejoras incrementales (pinceles de área/validadores/importación) sin reemplazar la estructura de mapa hasta medir sus límites.
- **Dificultad/riesgo:** media para herramientas de pintura; alta para biomas automáticos y compatibilidad de datos guardados.

### 5.5 IA de monstruos

- **Existe:** NPC con agresión, selección de objetivo, mapa de daño, alcance/visión, radio de reinicio y búsqueda de rutas A*.
- **Módulos:** `Intersect.Server.Core/Entities/Npc.cs`, `Intersect.Server.Core/Entities/Pathfinding`, `Framework/Intersect.Framework.Core/GameObjects/NPCs/NPCDescriptor.cs`, editor `frmNpc`.
- **Límite observado:** no se encontró un sistema general de profesiones/árboles de conducta visual; el comportamiento existente está centrado en persecución, ataque y reinicio.
- **Recomendación:** perfilar y documentar los estados existentes antes de ampliar tácticas de grupo, roles o comportamiento por bioma.
- **Dificultad/riesgo:** media-alta; coste de búsqueda por tick y sincronización con clientes.

### 5.6 Inventario y equipamiento

- **Existe:** bolsa, inventario, banco, ranuras de equipo, efectos/bonificaciones de objetos, papeldoll y mensajes de actualización de inventario.
- **Módulos:** `Intersect.Server.Core/Database/PlayerData/Players`, `Intersect.Server.Core/Entities/Player.cs`, `Framework/Intersect.Framework.Core/GameObjects/Items`, `Intersect.Client.Core/Interface/Game/Inventory` y formularios `frmItem`.
- **Recomendación:** adaptar requisitos y bonificaciones a los atributos nuevos solo después de cerrar el contrato de estadísticas.
- **Dificultad/riesgo:** media; los arrays de estadísticas de objetos y los datos persistidos dependen de enums compartidos.

### 5.7 Crafting y profesiones

- **Existe:** recetas, ingredientes y mesas de crafting, con condiciones y eventos comunes asociados.
- **Módulos:** `Framework/Intersect.Framework.Core/GameObjects/Crafting`, `Intersect.Server.Core/Networking/PacketHandler.cs`, `Intersect.Server.Core/Networking/PacketSender.cs`, `Intersect.Client.Core/Interface/Game/Crafting` y `frmCrafts`/`frmCraftingTables`.
- **Límite observado:** no se identificó un sistema transversal de profesiones con experiencia, rangos y especializaciones.
- **Recomendación:** reutilizar recetas/mesas y modelar profesiones como sistema aparte que las desbloquee, después de decidir progresión.
- **Dificultad/riesgo:** media-alta; depende de objetos, inventario, requisitos y persistencia.

### 5.8 Misiones y eventos

- **Existe:** objetivos/progreso de quests, condiciones, comandos y editor visual de quests/eventos.
- **Módulos:** `Framework/Intersect.Framework.Core/GameObjects/Quests`, `Framework/Intersect.Framework.Core/GameObjects/Events`, `Intersect.Server.Core/Database/PlayerData/Players/Quest.cs`, `Intersect.Client.Core/Interface/Game/QuestsWindow.cs`, `Intersect.Editor/Forms/Editors/Quest` y `Intersect.Editor/Forms/Editors/Events`.
- **Recomendación:** mantener los eventos como mecanismo de contenido y verificar compatibilidad de condiciones con los futuros atributos.
- **Dificultad/riesgo:** media; añadir estadísticas a condiciones puede afectar scripts y contenido ya creado.

### 5.9 Grupos, clanes, comercio

- **Existe:** party, guilds/clanes, rangos y permisos, invitaciones, banco de gremio, tiendas y paquetes de intercambio entre jugadores. Hay tablas y registros de actividad/intercambios.
- **Módulos:** `Intersect.Server.Core/Entities/Player.cs`, `Intersect.Server.Core/Database/PlayerData/Players/Guild.cs`, paquetes `Framework/Intersect.Framework.Core/Network/Packets`, `Intersect.Client.Core/Interface/Game/PartyWindow.cs`, `GuildWindow.cs` y formularios de tiendas.
- **Recomendación:** conservar validaciones del servidor y revisar transacciones/duplicación de objetos antes de ampliar economía.
- **Dificultad/riesgo:** media-alta; abuso de paquetes, concurrencia y consistencia entre jugadores.

### 5.10 Rendimiento y sincronización multijugador

- **Existe:** transporte LiteNetLib, protocolo de paquetes compartidos, servidor que aplica acciones de juego y envío de actualizaciones a jugadores en proximidad/mapa.
- **Módulos:** `Intersect.Network`, `Intersect.Server.Core/Networking/PacketHandler.cs`, `Intersect.Server.Core/Networking/PacketSender.cs`, `Intersect.Server/Networking`, `Intersect.Client.Core/Networking/PacketHandler.cs`.
- **Límite observado:** no se ejecutaron pruebas de carga ni cliente-servidor en esta fase; la compilación no confirma latencia, pérdidas, reconexión ni comportamiento con muchos jugadores.
- **Recomendación:** crear pruebas de protocolo y perfiles de carga antes de optimizar o alterar el formato de paquetes.
- **Dificultad/riesgo:** alta; las modificaciones deben permanecer compatibles entre cliente, servidor y editor.

### 5.11 Persistencia y seguridad

- **Existe:** SQLite/MySQL, Entity Framework, migraciones, serialización de valores del jugador, registro de actividad y validación de acciones en el servidor.
- **Módulos:** `Intersect.Server.Core/Database`, `Intersect.Server/Database`, `Intersect.Server.Core/Entities/Player.Database.cs`, `Intersect.Server/Program.cs`, `Intersect.Network/LiteNetLib`.
- **Dependencia SQLite:** la restauración resolvió `SQLitePCLRaw.lib.e_sqlite3` 2.1.14 junto con `bundle_green` 2.1.10, y la compilación posterior terminó correctamente sin errores. La persistencia de la clase, el mapa, la cuenta y el personaje de prueba se confirmó tras reiniciar.
- **Licencias del motor:** cliente MIT; servidor y editor GPL-3.0 según sus archivos de licencia; otros proyectos pueden tener licencia propia. Revisar cada componente y assets antes de distribuir.
- **Dificultad/riesgo:** alta; cambios de esquema o serialización requieren copias de respaldo y migraciones para ambas bases de datos.

### 5.12 Importación masiva de contenido

- **Existe:** descriptores de objetos y datos serializados para contenido del juego; no se identificó un importador general por lotes con validación/reporte en la inspección.
- **Módulos relacionados:** descriptores en `Framework/Intersect.Framework.Core/GameObjects`, base de datos de juego en `Intersect.Server.Core/Database/GameData` y listas/editores en `Intersect.Editor/Forms/Editors`.
- **Recomendación:** diseñar un importador como herramienta independiente que valide identificadores, referencias, sprites, categorías y errores antes de guardar.
- **Dificultad/riesgo:** media-alta; referencias rotas podrían dejar contenido inválido o migraciones parciales.

## 6. Compilación, pruebas y verificaciones

### Compilación

- La compilación inicial informada de `Intersect.sln` con .NET 8.0.427 fue correcta en **27,1 s**, con **2.179 advertencias**.
- Tras la restauración válida de SQLitePCLRaw 2.1.14, la compilación Debug terminó correctamente en **13,9 s, con 79 advertencias y sin errores**. Es un build incremental, así que no se compara directamente el conteo de advertencias con el inicial.
- El resumen de compilación incluyó cliente, servidor, editor, modo de un jugador y bibliotecas del motor.
- El primer build base (antes de actualizar ImageSharp) se había informado con 2.196 advertencias. Tras ImageSharp 3.2.0, el resumen compartido bajó a 2.179.
- No se corrigieron las advertencias generales de C# en esta fase.

### Pruebas y ejecución

- Hay pruebas unitarias en `Intersect.Tests` y pruebas del servidor sobre experiencia y niveles en `Intersect.Tests.Server`.
- Los proyectos `Intersect.Tests.Client`, `Intersect.Tests.Network`, `Intersect.Tests.Editor` y `Intersect.Tests.Client.Framework` contienen stubs o cobertura limitada; no constituyen pruebas de integración cliente-servidor.
- El primer intento de `dotnet test` no pudo compilar porque buscó `Intersect.Network\\bin\\Release\\keys\\network.handshake.bkey.pub`, que no existía.
- En un segundo intento con configuración Debug, el usuario reportó **44 pruebas correctas, 0 errores y 0 omitidas** en 4,3 s; la compilación terminó correctamente con 5 advertencias en 10,6 s. No se proporcionó el detalle de esas advertencias, así que aún no se confirma si ImageSharp encontró la licencia.
- Conexión y creación inicial: el log del cliente registra conexión y aprobación del servidor en `127.0.0.1:5400`. El usuario confirmó la creación de cuenta y personaje, el sprite visible y que ambos datos seguían disponibles tras reiniciar el servidor. También accedió al editor y guardó un mapa simple; la captura posterior confirma que el cliente muestra el terreno, las zonas de piedra y el sprite. La persistencia SQLite de la clase, el mapa, la cuenta y el personaje queda confirmada.
- Seguimiento actualizado: SQLite 3.54.0 del build tiene `DQS=0`. Se corrigieron los literales con comillas dobles en `20221016001123_MultipleBonusEffectsMigration` y en las cuatro actualizaciones de `20221119214350_ItemProperties`. Las consultas corregidas pasan con la misma DLL en memoria. `Intersect.Server.Core` y `Intersect.Server` compilaron sin errores. El usuario confirmó esquema completo, arranque, escucha UDP 5400 y persistencia SQLite tras reinicio. UPnP no detectó un router compatible y HTTPS mostró aviso de certificado; no impidieron conexión local.

## 7. Riesgos que deben resolverse

1. **Versión beta:** la base coincide con los ejecutables, pero no es estable.
2. **SQLite nativo/migraciones:** NuGet restauró `lib.e_sqlite3` 2.1.14 y `bundle_green` 2.1.10. Dos migraciones se ajustaron para `DQS=0`; el servidor completó su esquema y mantuvo la clase/mapa de prueba después del reinicio.
3. **ImageSharp:** el build informado permite Debug sin clave; Release requiere una clave válida. Resolverlo antes de compilar una versión Release.
4. **Datos de atributos:** arrays persistidos y descriptores se indexan por el enum; el orden/tamaño nuevo requiere migración y compatibilidad hacia atrás.
5. **Protocolo cliente-servidor:** modificaciones de combate/estadísticas deben coordinar paquetes, validación y representación.
6. **Avisos de compilador:** el build incremental más reciente informó 79; priorizar alertas de dependencias y fallos funcionales, no limpiar todas sin clasificación.
7. **Licencias:** los componentes de Intersect usan licencias distintas y los recursos tienen licencias separadas.
8. **Fork remoto:** `origin` apunta al repositorio upstream; todavía no se configuró un remoto personal.

## 8. Plan de implementación recomendado

### Fase 1A — Cerrar preparación

1. Configurar una clave ImageSharp válida o evaluar una alternativa antes de compilar Release.
2. Confirmar que la compilación ya no muestra el aviso de licencia de ImageSharp y revisar la generación de claves antes de usar Release.
3. Preparar el remoto personal del fork cuando el usuario lo cree/seleccione, conservando `origin` como upstream.

### Fase 1B — Diseño de cambios

1. Documentar el flujo de datos actual de atributos desde editor/descriptores hasta servidor, cliente, paquetes y persistencia.
2. Definir semántica, límites y fórmulas de Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría.
3. Preparar estrategia de migración de personajes, clases, NPC, equipo, hechizos, contenido y fórmulas.
4. Definir pruebas de regresión y criterios de aceptación antes de implementar.

### Fase 2 — Implementación de atributos

Comenzar solo con autorización del usuario. Cambiar un sistema transversal a la vez y mantener cliente, servidor, editor, contenido y persistencia sincronizados.

## 9. Decisiones confirmadas del juego

- Nombre oficial: **El Legado de la Última Guerra** (la carpeta local conserva el nombre `ultima guerra`).
- Género: MMORPG 2D cenital de fantasía medieval.
- Atributos futuros: Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría.
- Gráficos iniciales: sprites pixel art estilo RPG Maker XP.
- Dirección futura: desplazamiento tradicional y combate con capas de brazos/armas/equipo.
- Prioridad: estabilidad, mecánicas y herramientas antes de una renovación artística.
- En esta fase no se reemplazan estadísticas, combate ni editor.
