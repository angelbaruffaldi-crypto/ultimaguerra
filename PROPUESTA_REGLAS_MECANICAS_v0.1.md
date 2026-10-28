# Propuesta de reglas mecánicas — versión 0.1

**Proyecto:** El Legado de la Última Guerra  
**Fecha:** 28 de octubre de 2026  
**Estado:** propuesta de diseño preparada; todavía no aplicada al motor.

## Objetivo

Definir una primera estructura de atributos que encaje con el combate de Intersect sin confundir atributos asignables con resultados derivados como ataque, defensa o velocidad. La propuesta favorece una primera versión PvE clara, con cálculos en el servidor y constantes configurables para balancear después.

## Decisión técnica recomendada

Separar:

1. **Atributos primarios del personaje:** los cinco valores que el jugador y el editor entienden y pueden asignar.
2. **Estadísticas de combate derivadas:** resultados calculados por clase, nivel, equipo, atributos y efectos.

Así, el juego mantiene cinco atributos propios y puede seguir usando conceptos derivados como ataque físico, defensa, poder mágico, resistencia mágica y velocidad. El número u orden de los atributos primarios no debe determinar por sí solo la posición de datos de combate.

## Atributos primarios propuestos

| Atributo | Función principal propuesta | No debería controlar directamente |
|---|---|---|
| **Fuerza** | Escalado de daño de armas cuerpo a cuerpo y requisitos de armas pesadas. | Daño mágico o velocidad de movimiento. |
| **Destreza** | Precisión con armas a distancia y requisitos de armas ligeras. | Defensa física total o poder mágico. |
| **Inteligencia** | Escalado del daño de hechizos y habilidades mágicas. | Defensa física o velocidad de movimiento. |
| **Agilidad** | Evasión y cadencia de ataque, con límites para evitar ventajas descontroladas. | Daño mágico o defensa física directa. |
| **Sabiduría** | Reserva/regeneración de maná y resistencia mágica. | Daño de armas físicas. |

La lista asigna una función principal a cada atributo y evita que todos aumenten daño. La precisión, evasión y cadencia deben limitarse en el servidor; sus coeficientes se ajustarán en una prueba de combate y no se fijan aquí.

## Estadísticas derivadas propuestas

- **Daño físico:** daño base del arma más escalado por Fuerza para armas cuerpo a cuerpo o por Destreza para armas a distancia.
- **Precisión:** valor base más aporte acotado de Destreza; se contrasta con la evasión del objetivo.
- **Evasión:** valor base más aporte acotado de Agilidad.
- **Cadencia:** tiempo base del arma/clase reducido de manera limitada por Agilidad; se establece un tiempo mínimo en servidor.
- **Poder mágico:** escalado por Inteligencia.
- **Defensa física:** armadura, escudo, clase/nivel y efectos; deja de ser un atributo asignable.
- **Resistencia mágica:** Sabiduría, equipo y efectos.
- **Vida:** clase, nivel y efectos; no se vincula a Fuerza en el primer prototipo para no hacer que una inversión ofensiva también sea obligatoria para sobrevivir.
- **Maná:** clase/nivel y Sabiduría; el coste y la recuperación se validan en el servidor.

## Resolución de daño

Se recomienda conservar la estructura de Intersect —daño base, escalado, crítico y mitigación configurable— mientras se cambian las fuentes de escalado:

```text
daño bruto = daño base + atributo de escalado × factor del arma/habilidad
daño final = daño bruto × crítico × mitigación del objetivo
```

El servidor determina objetivo, alcance, precisión, crítico, coste, enfriamiento y daño. El cliente solo solicita acciones y muestra el resultado comunicado por el servidor.

Las fórmulas deben usar nombres explícitos (`Strength`, `Dexterity`, etc.) y conservar alias de lectura para contenido antiguo durante la transición. No se deben reinterpretar expresiones guardadas sin una migración registrada.

## Progresión

- Mantener la progresión por nivel y clase existente durante el primer prototipo, cambiando únicamente qué cinco atributos representan esos valores.
- Configurar distribución inicial y crecimiento automático por clase en el editor.
- Conservar la asignación manual de puntos y el límite configurable ya existente hasta probar el nuevo combate.
- Ajustar coeficientes, topes de precisión/evasión/cadencia y curva de puntos después de pruebas de combate; no duplicar números en cliente y servidor.

Esto evita inventar una curva de niveles nueva antes de comprobar que el servidor y el editor pueden representar los cinco atributos correctamente.

## Objetos, hechizos y enemigos

- Armas cuerpo a cuerpo: daño base y atributo de escalado Fuerza.
- Armas a distancia: daño base, alcance y atributo de escalado Destreza.
- Hechizos: coste de maná y escalado por Inteligencia.
- Armaduras/escudos: defensa física derivada, sin convertirla en un sexto atributo asignable.
- Efectos de hechizo: pueden aumentar o reducir atributos primarios o resultados derivados, con duración y acumulación definidas por el servidor.
- Enemigos: usan la misma resolución de ataque/defensa del servidor; el editor ofrece plantillas y valores iniciales, no fórmulas distintas ocultas.

## Datos antiguos y migración

No reutilizar por índice el arreglo anterior `[Attack, AbilityPower, Defense, MagicResist, Speed]` como `[Fuerza, Destreza, Inteligencia, Agilidad, Sabiduría]`. Eso podría convertir defensa en destreza y desplazar el resto de datos sin que la base de datos lo detecte.

Propuesta de migración:

1. Versionar el formato de atributos y guardar/leer por identificador estable, no depender únicamente de la posición del enum.
2. Respaldar las bases de datos SQLite y MySQL antes de migrar.
3. Convertir explícitamente `Attack → Fuerza`, `AbilityPower → Inteligencia`, `Speed → Agilidad` y `MagicResist → Sabiduría` solo como candidatos iniciales.
4. Conservar el `Defense` anterior en un campo temporal de legado o convertirlo mediante una regla de migración definida, sin reasignarlo silenciosamente a Destreza.
5. Inicializar Destreza con valores de clase/nivel o reasignar puntos al jugador solo con una política de migración documentada.
6. Validar totales, límites y reversión en una copia de prueba antes de modificar datos existentes.

La conversión de estadísticas de personajes y la conversión de clases/NPC/objetos son problemas separados; una migración debe cubrir ambos.

## Pruebas de aceptación para el primer prototipo

- Cambiar un atributo afecta solo los resultados definidos en esta propuesta.
- Una configuración mayor nunca rompe los límites de precisión, evasión ni cadencia.
- El cálculo autoritativo es igual para cliente, jugador y NPC; el cliente no puede falsificarlo.
- El equipo, hechizos, puntos y atributos se guardan y recuperan tras reiniciar.
- Las fórmulas antiguas se detectan y migran o producen un error legible; no se reinterpretan silenciosamente.
- La migración de datos tiene una copia restaurable y no pierde puntos de personaje.
- Los editores de clase, NPC, objetos y hechizos muestran los nuevos campos y guardan los identificadores correctos.

## Alcance de esta propuesta

Esta versión define responsabilidades, no números finales de balance. No agrega profesiones, carga de peso, árbol de talentos, PvP competitivo ni nuevas animaciones. Primero se implementaría un combate PvE mínimo y se mediría con un jugador, un enemigo, una arma cuerpo a cuerpo, un arma a distancia, un hechizo y una pieza de armadura.

Esta propuesta queda como base provisional del diseño. La siguiente tarea técnica es convertir la matriz de migración en cambios pequeños por servidor, datos compartidos, editor, cliente y persistencia; los números de balance se calibrarán en la prueba vertical.
