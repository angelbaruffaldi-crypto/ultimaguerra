# Contrato de datos de atributos — versión 1

**Fecha:** 28 de octubre de 2026  
**Estado:** diseño técnico; todavía no aplicado al código ni a bases de datos.  
**Reglas mecánicas relacionadas:** `PROPUESTA_REGLAS_MECANICAS_v0.1.md`.

## 1. Separación de conceptos

- `PrimaryAttribute`: valores que el jugador asigna: Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría.
- `CombatStat`: resultados derivados y modificables que usa el motor: ataque, poder mágico, defensa física, resistencia mágica y velocidad/cadencia.
- Los atributos primarios no reutilizan el enum ordinal `Intersect.Enums.Stat` como si sus posiciones fueran intercambiables.
- Los resultados derivados se calculan en el servidor desde atributos, clase, nivel, equipo y efectos. El cliente muestra resultados sincronizados; no decide daño, precisión, evasión ni costes.

## 2. Identificadores persistidos

Se guardan claves estables en inglés para que renombrar una etiqueta en español no cambie los datos:

| Clave persistida | Etiqueta de interfaz |
|---|---|
| `strength` | Fuerza |
| `dexterity` | Destreza |
| `intelligence` | Inteligencia |
| `agility` | Agilidad |
| `wisdom` | Sabiduría |

La interfaz y el editor localizan etiquetas por separado. En memoria se usa un tipo compartido con identificadores numéricos explícitos y estables; no se deriva el ID de la posición actual del enum. En red el paquete lleva el ID estable y la versión de protocolo.

## 3. Formato JSON versionado

Los valores de atributo se serializan como objeto, nunca como arreglo posicional:

```json
{
  "schemaVersion": 1,
  "values": {
    "strength": 10,
    "dexterity": 10,
    "intelligence": 10,
    "agility": 10,
    "wisdom": 10
  }
}
```

El mismo formato se usa para valores base y puntos asignados, en campos distintos. Los valores derivados calculados no se escriben en este bloque.

### Persistencia de personajes

Agregar campos de texto separados para atributos base y asignaciones, por ejemplo `PrimaryAttributesJson` y `AttributePointAllocationsJson`. Conservar las columnas actuales `BaseStats` y `StatPointAllocations` para los valores derivados que aún consume el combate. Antes de convertir, archivar los arreglos originales completos en una copia legible (`LegacyStatsJson` y `LegacyStatPointsJson` o una tabla de respaldo). Retirar columnas heredadas solo en una migración posterior, una vez que la versión nueva lleve tiempo validada.

### Clases y NPC

En los descriptores de contenido se agregan objetos versionados para valores base y crecimiento de atributos. Durante la conversión, los cuatro valores que tienen correspondencia se copian a atributos; Defense se conserva como valor derivado de clase/NPC. Las copias originales permanecen archivadas.

### Objetos y hechizos

Los modificadores nuevos identifican explícitamente si afectan un atributo primario o una estadística derivada. El lector antiguo mantiene compatibilidad con los arreglos actuales hasta que contenido, editor y servidor estén migrados juntos.

## 4. Reglas del lector/escritor

1. Si el JSON raíz es un arreglo de enteros, tratarlo como versión 0 y usar exclusivamente el orden antiguo documentado.
2. Si es un objeto, exigir `schemaVersion` y validar todas las claves conocidas.
3. Leer claves ausentes desde el valor por defecto del descriptor; no convertirlas silenciosamente a cero.
4. En escritura, emitir solo el objeto versionado v1 con las cinco claves ordenadas para facilitar diffs.
5. Si aparece una versión futura, una clave desconocida o JSON corrupto, detener la carga de esa entidad con un error identificable y conservar el dato original. No reemplazarlo por valores vacíos.
6. Validar valores y límites en el servidor al crear, migrar, cargar, asignar puntos y recibir paquetes.

## 5. Conversión determinista propuesta

Para personajes ya creados, la migración hará una reasignación controlada:

- Guardar los arreglos originales completos en copia de respaldo antes de convertir.
- Copiar `BaseStats` y `StatPointAllocations` antiguos al respaldo antes de convertir.
- Convertir en base primaria `Attack→Fuerza`, `AbilityPower→Inteligencia`, `Speed→Agilidad` y `MagicResist→Sabiduría`; inicializar Destreza desde la clase migrada. Conservar Defense como valor derivado y no trasladarlo a Destreza.
- Convertir las asignaciones de esos cuatro valores a sus nuevos atributos. Sumar la antigua asignación de Defense a `StatPoints` no asignados y poner a cero el arreglo viejo en la fila activa, para no duplicar puntos.
- Registrar por personaje la versión de origen, el total anterior, los puntos devueltos y cualquier valor legado no convertido.
- Si faltan clase, datos o puntos no cuadran, no migrar esa fila automáticamente; incluirla en el informe para reparación.

Para clases y NPC, convertir valores base y crecimiento de los cuatro candidatos (Attack→Fuerza, AbilityPower→Inteligencia, Speed→Agilidad, MagicResist→Sabiduría) y conservar Defense y su crecimiento como estadística derivada. Destreza usa el valor de plantilla definido para cada clase/NPC; si no existe, la migración deja ese contenido reportado para completar en el editor, no inventa una cifra.

Para objetos, hechizos, rangos aleatorios y fórmulas, la conversión genera un informe de equivalencias. Cualquier selección antigua de Defense/Attack usada como escalado que no tenga un destino inequívoco queda pendiente de revisión de contenido, no de conversión silenciosa.

## 6. Compatibilidad de fórmulas y protocolo

- La primera versión del servidor debe poder leer fórmulas existentes y registrar cada nombre heredado utilizado.
- Las fórmulas nuevas exponen nombres explícitos (`A_Strength`, `A_Dexterity`, `A_Intelligence`, `A_Agility`, `A_Wisdom`) además de salidas derivadas (`V_PhysicalDefense`, etc.).
- No eliminar alias anteriores hasta que el informe de contenido confirme que no quedan fórmulas antiguas o hasta que la conversión las haya escrito y validado.
- Cambiar el contrato del paquete de mejora de atributo requiere incrementar el protocolo. Un cliente con versión incompatible se rechaza antes de entrar al mundo.

## 7. Plan de implementación seguro

1. Añadir tipos compartidos e IDs estables, sin cambiar todavía el combate existente.
2. Añadir serializador v1 y lector v0/v1 con registro claro de errores.
3. Añadir columnas de atributos primarios en SQLite y MySQL y reflejarlas en el modelo.
4. Implementar conversión en modo de simulación: producir informe, no escribir filas.
5. Revisar informe contra una copia de la base del usuario; conservar respaldo restaurable.
6. Migrar descriptores de clases/NPC y adaptar el editor.
7. Conectar cálculo primario→derivado en servidor y fórmulas; actualizar paquetes y vistas del cliente.
8. Habilitar escritura de v1, probar la migración en copias de ambas bases y solo entonces usarla en una base activa.
9. Tras una versión estable, considerar eliminar columnas y alias heredados en un cambio separado.

**Criterio de salida:** las cinco claves se guardan/leen de forma estable, las filas heredadas se reportan sin pérdida, SQLite y MySQL dan el mismo resultado, y clientes incompatibles no pueden enviar índices ambiguos.
