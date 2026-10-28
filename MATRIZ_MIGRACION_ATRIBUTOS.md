# Matriz de migración — atributos y mecánicas

**Fecha:** 28 de octubre de 2026  
**Estado:** inventario inicial de superficies completado; plan de migración por probar.  
**Diseño de referencia:** `PROPUESTA_REGLAS_MECANICAS_v0.1.md`.

## Inventario de datos afectados

| Área | Datos actuales | Ubicación principal | Trabajo necesario |
|---|---|---|---|
| Identidad compartida | Enum ordinal `Attack`, `AbilityPower`, `Defense`, `MagicResist`, `Speed` | `Framework/Intersect.Framework.Core/Entities/Stat.cs` | Introducir identificadores para atributos primarios y mantener separados los valores derivados de combate. |
| Jugadores y entidades | `BaseStats`, `StatPointAllocations` serializados en columnas como arreglos de enteros | `Intersect.Server.Core/Entities/Entity.cs`; modelo de jugador SQLite/MySQL | Versionar los datos; migrar con nombres/índices explícitos y conservar copia recuperable. |
| Clases | `BaseStat`, `StatIncrease`, `ScalingStat` | `Framework/Intersect.Framework.Core/GameObjects/PlayerClass/ClassDescriptor.cs`; `Intersect.Editor/Forms/Editors/frmClass.cs` | Cambiar valores iniciales, crecimiento y opciones de escalado del editor. |
| NPC | Arreglo de estadísticas y estadística de escalado | Descriptor de NPC; `Intersect.Editor/Forms/Editors/frmNpc.cs` | Convertir plantillas y revisar escalado por nivel. |
| Combate | Variables de fórmulas para atacante/objetivo, estadística de escalado, daño físico/mágico/verdadero | `Intersect.Server.Core/General/Formulas.cs` y configuración del servidor | Migrar variables de fórmula sin reinterpretar texto guardado; ejecución autoritativa en servidor. |
| Hechizos | `StatDiff`, `PercentageStatDiff`, `ScalingStat` | Descriptores de hechizo; `Intersect.Editor/Forms/Editors/frmSpell.cs` | Separar bonificaciones de atributos y de estadísticas derivadas; actualizar escalado mágico. |
| Objetos/equipo | `StatsGiven`, porcentajes, `StatRanges`, `ScalingStat` | Descriptores de objetos y `EquipmentProperties`; `Intersect.Editor/Forms/Editors/frmItem.cs` | Redefinir qué bonifica un arma, armadura o consumible; conservar defensa como resultado de armadura si se adopta la propuesta. |
| Eventos/condiciones | Condiciones que consultan estadísticas, cambios temporales y reinicio de puntos | `Intersect.Server.Core/Entities/Events`, `Framework/.../GameObjects/Conditions` | Actualizar editor, validación y contenido serializado. |
| Paquetes de red | Índices de mejora y listas de valores de entidad | `UpgradeStatPacket`, `EntityStatsPacket`, `Intersect.Server.Core/Networking/PacketSender.cs` | Actualizar protocolo y asegurar que el servidor valide el tipo de atributo recibido. Cliente y servidor deben usar la misma versión. |
| Interfaz/localización | Etiquetas, controles, límites y botones de asignación | `CharacterWindow`, localización del cliente y `Intersect.Editor/General/Globals.cs` | Mostrar cinco atributos primarios; reservar los valores derivados para paneles de combate si corresponde. |
| Persistencia | Arrays JSON en SQLite y MySQL; contenido de juego y jugadores separados | `Intersect.Server.Core/Database`, `Intersect.Server/Migrations` | Crear migración equivalente para los dos proveedores y probar respaldo/restauración. |

## Conversión candidata de datos antiguos

La propuesta no justifica una conversión automática completa. Las correspondencias candidatas son:

| Dato anterior | Posible destino | Estado |
|---|---|---|
| `Attack` | Fuerza | Coincidencia razonable para armas cuerpo a cuerpo; revisar objetos y fórmulas. |
| `AbilityPower` | Inteligencia | Coincidencia razonable para magia; revisar hechizos y clases. |
| `Speed` | Agilidad | Coincidencia parcial: también afecta movimiento y cadencia en el código actual. |
| `MagicResist` | Sabiduría | Coincidencia parcial; Sabiduría también asumiría maná según la propuesta. |
| `Defense` | Sin destino primario directo | Mantener como campo legado hasta derivar defensa de armadura/clase y demostrar que no se pierden puntos. |
| Destreza | Sin dato anterior | Inicializar por plantilla de clase/NPC; no se puede reconstruir de manera fiable desde el arreglo antiguo. |

Por lo tanto, **no se debe migrar el arreglo existente cambiando el orden del enum**. Para personajes de prueba se puede decidir reinicio controlado. Para personajes reales se necesitará conservar el arreglo anterior en el respaldo, calcular un informe de conversión y definir cómo se reasignan puntos antes de ejecutar la migración.

## Secuencia propuesta de migración

1. Congelar un commit base y sacar copias de SQLite y MySQL; nunca probar sobre la única base disponible.
2. Definir versión del esquema de atributos y una representación con identificadores estables.
3. Añadir lectura compatible con la versión antigua y una rutina de conversión que produzca un informe de filas convertidas, valores sin destino y puntos reasignados.
4. Migrar primero datos de contenido (clases, NPC, objetos, hechizos, condiciones y fórmulas) en una copia de prueba.
5. Migrar personajes preservando atributos reconocibles y guardando Defensa antigua; verificar totales y límites.
6. Actualizar el protocolo cliente-servidor como un cambio coordinado y rechazar clientes con protocolo incompatible.
7. Abrir los contenidos migrados en el editor y probar creación, nivel, equipo, hechizo, movimiento, combate y reinicio.
8. Ensayar restauración; solo después considerar la base real.

## Riesgos y medidas

- **Desplazamiento de índices:** sustituir el enum en el mismo arreglo convierte valores válidos en atributos equivocados. Medida: formato/versionado explícito y pruebas de migración.
- **Fórmulas guardadas:** pueden referirse a `A_Attack`, `V_Defense`, `A_Speed` y otros nombres. Medida: buscar y traducir expresiones en modo de simulación, reportando fórmulas que no se puedan convertir.
- **Escalado de contenido:** armas, hechizos y NPC almacenan índices de escalado. Medida: convertirlos en una tarea separada del arreglo de jugadores.
- **Desincronización de red:** un cliente antiguo puede enviar un índice que el servidor interprete de otro modo. Medida: versión de protocolo y bloqueo de versiones incompatibles.
- **Diferencia entre bases:** SQLite y MySQL deben producir el mismo contenido migrado. Medida: ejecutar la misma batería en copias de ambos formatos.
- **Pérdida de Defensa:** la propuesta deja de asignarla como atributo primario. Medida: mantener temporalmente el valor legado y no descartarlo hasta que la defensa derivada esté validada.

## Próximo entregable técnico

Convertir esta matriz en un plan de implementación por commit, empezando por los tipos compartidos y el servidor, luego contenido/editor, interfaz/red y finalmente migración. El primer cambio de código debe incluir lectura/escritura versionada y una prueba de conversión de datos de muestra; no debe alterar mecánicas de movimiento hasta que Agilidad y límites estén implementados a ambos lados.
