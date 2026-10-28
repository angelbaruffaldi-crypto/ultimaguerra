# Hoja de ruta — El Legado de la Última Guerra

**Actualizada:** 28 de octubre de 2026  
**Repositorio:** `Intersect-Engine-v0.8.0-beta.582`  
**Rama:** `ultima-guerra`  
**Estado global:** preparación técnica completada; diseño de mecánicas en curso.

Esta hoja es el registro principal del avance. Cada tarea cambia a **COMPLETADO** cuando hay evidencia verificable. Las decisiones de diseño que afecten el juego se anotan como pendientes hasta que exista una propuesta revisable; no se asignan efectos a los atributos solo por sus nombres.

## Resumen de estado

| Fase | Estado | Resultado / condición para cerrarla |
|---|---|---|
| 0. Resguardar el código y preparar el fork | **COMPLETADO** | Código fuente versionado, rama propia publicada y `upstream` conservado. |
| 1. Obtener una base técnica comprobada | **COMPLETADO** para Debug; **PENDIENTE** para Release | Pruebas y flujo local pasan; faltan resolver claves de red y verificar ImageSharp en Release. |
| 2. Diseñar las reglas principales del juego | **EN CURSO** | Hay un primer mapa de impacto de estadísticas; falta especificar atributos, combate y progresión. |
| 3. Diseñar datos y migraciones | **PENDIENTE** | Contratos, compatibilidad y plan de migración revisados antes de implementar. |
| 4. Implementar una primera versión jugable | **PENDIENTE** | Bucle principal completo y probado en cliente/servidor/editor. |
| 5. Crear mundo y contenido inicial | **PENDIENTE** | Zona inicial, enemigos, objetos, misiones y recorrido jugable. |
| 6. Adaptar los sprites ampliados | **PENDIENTE** | Archivos LPC probados por acción/dirección; se decide si basta la configuración existente o hay que extender el renderizador. |
| 7. Preparar pruebas multijugador y operación | **PENDIENTE** | Copias de seguridad, seguridad, red, carga y recuperación comprobadas. |
| 8. Preparar una versión de prueba y lanzamiento | **PENDIENTE** | Build Release reproducible, licencias verificadas, paquete y guía de instalación. |

## Fase 0 — Resguardar el código y preparar el fork

- **COMPLETADO** — Fuente de Intersect `v0.8.0-beta.582` descargada con submódulos.
- **COMPLETADO** — Rama `ultima-guerra` basada en el tag elegido; `origin` es el fork personal y `upstream` apunta al motor oficial.
- **COMPLETADO** — Rama publicada en GitHub y sincronizada en el último registro.
- **COMPLETADO** — Cambios iniciales del motor y correcciones de SQLite registrados en commits, separados del tag original.
- **COMPLETADO** — Documentación de estado, auditoría y esta hoja de ruta bajo control de Git.

## Fase 1 — Obtener una base técnica comprobada

- **COMPLETADO** — Compilación Debug informada como correcta.
- **COMPLETADO** — `Intersect.Tests.Server`: 44 correctas, 0 fallidas, 0 omitidas; resultado aportado por el usuario.
- **COMPLETADO** — Servidor SQLite inicia, termina migraciones y conserva datos de cuenta/personaje al reiniciar.
- **COMPLETADO** — Cliente conecta al servidor local; el usuario crea cuenta y personaje.
- **COMPLETADO** — Editor guarda un mapa simple y el cliente muestra terreno, zonas y personaje.
- **PENDIENTE** — Obtener una compilación Release reproducible. El intento anterior no encontró `Intersect.Network/bin/Release/keys/network.handshake.bkey.pub`.
- **PENDIENTE** — Revisar la salida detallada de Release para confirmar la licencia de ImageSharp. No guardar claves ni valores secretos en el repositorio.
- **PENDIENTE** — Clasificar las advertencias de dependencia y compatibilidad antes de distribuir; el último resumen compartido indicaba 343 advertencias en 31,1 s.

## Fase 2 — Diseñar las reglas principales del juego

### 2.1 Estadísticas y progresión

- **COMPLETADO** — Confirmados los cinco nombres: Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría.
- **COMPLETADO** — Mapeo transversal inicial del sistema actual documentado en `AUDITORIA_PREPARACION.md`: enum, fórmulas, movimiento, jugador, objetos, hechizos, clases/NPC, editor, interfaz, eventos y persistencia.
- **PENDIENTE** — Preparar una propuesta de diseño para cada atributo: qué representa, qué acciones afecta y cómo se calcula.
- **PENDIENTE** — Definir valores iniciales, crecimiento por nivel, puntos asignables, límites, reinicio y reglas de clase.
- **PENDIENTE** — Definir cómo equipo, hechizos, efectos y enemigos interactúan con los nuevos atributos.

### 2.2 Combate

- **PENDIENTE** — Acordar combate cuerpo a cuerpo, distancia y magia; daño, defensas, alcance, cadencia, interrupciones y estados.
- **PENDIENTE** — Definir muerte, reaparición, pérdida/recuperación de objetos y reglas PvP/PvE.
- **PENDIENTE** — Fijar fórmulas y variables que estarán disponibles en el editor sin permitir que el cliente decida resultados.

### 2.3 Bucle principal y alcance

- **PENDIENTE** — Describir el recorrido de un jugador: creación, exploración, combate, recompensas, crecimiento y objetivos.
- **PENDIENTE** — Elegir el alcance de la primera versión jugable: contenido mínimo, número de zonas, clases, enemigos y duración de una sesión.
- **PENDIENTE** — Determinar qué sistemas originales de Intersect se mantienen, se configuran o se reemplazan.

**Cierre de fase:** reglas principales documentadas con ejemplos y criterios de prueba, antes de cambiar código de mecánicas.

## Fase 3 — Diseñar datos y migraciones

- **PENDIENTE** — Crear una tabla campo por campo entre datos actuales de Intersect y el modelo del juego.
- **PENDIENTE** — Diseñar compatibilidad para jugadores, clases, NPC, objetos, hechizos, condiciones/eventos y fórmulas existentes.
- **PENDIENTE** — Definir migraciones y recuperación para SQLite y MySQL; probarlas con una copia de datos, nunca con la única copia.
- **PENDIENTE** — Definir versiones cliente-servidor y comportamiento cuando sus datos o protocolos no coincidan.
- **PENDIENTE** — Especificar pruebas de regresión, aceptación y casos de fallo antes de implementar.

**Cierre de fase:** plan de migración revisado y reversión demostrada en una copia de prueba.

## Fase 4 — Implementar una primera versión jugable

Cada subfase debe compilar y tener pruebas de servidor antes de continuar a la siguiente.

1. **PENDIENTE — Sistema de atributos:** enum/tipos compartidos, cálculo de servidor, progresión, datos persistidos, paquetes, interfaz y editor.
2. **PENDIENTE — Combate básico:** un ataque cuerpo a cuerpo, un enemigo, daño calculado en servidor, derrota y reaparición.
3. **PENDIENTE — Habilidades:** coste, requisitos, enfriamiento, alcance, objetivos, efectos y sincronización cliente-servidor.
4. **PENDIENTE — Equipo e inventario:** armas/armadura, requisitos, bonificaciones, botín y validación contra duplicación o paquetes manipulados.
5. **PENDIENTE — NPC y progresión:** comportamiento mínimo, experiencia, niveles, recompensas y ajuste de dificultad.
6. **PENDIENTE — Interfaz y editor:** formularios para configurar el contenido sin desalinear lo que valida el servidor.

**Cierre de fase:** un jugador puede crear personaje, explorar una zona, combatir, obtener una recompensa, equiparla y progresar; los datos sobreviven al reinicio.

## Fase 5 — Crear mundo y contenido inicial

- **PENDIENTE** — Definir premisa, tono, mapa de la zona inicial y objetivos del primer capítulo.
- **PENDIENTE** — Crear mapas y puntos de transición con colisiones, eventos y rutas de prueba.
- **PENDIENTE** — Crear un conjunto mínimo de enemigos, NPC, objetos, habilidades y misiones.
- **PENDIENTE** — Revisar dificultad, claridad de objetivos y tiempo de recorrido con una prueba de juego.

## Fase 6 — Adaptar sprites y presentación

- **COMPLETADO** — `masculino.png` y `femenino.png` están en `Client and Editor/resources/entities`.
- **COMPLETADO** — `arco.png`, `sable.png`, `lanza.png`, `pantalones cuero.png`, `baston.png` y `pechera cuero.png` están en `Client and Editor/resources/paperdolls`.
- **COMPLETADO** — Dimensiones registradas en `AUDITORIA_PREPARACION.md`.
- **PENDIENTE** — Versionar o respaldar la carpeta de recursos: `Client and Editor` está fuera del repositorio Git del motor.
- **PENDIENTE** — Verificar licencia/procedencia de cada conjunto antes de una distribución pública.
- **PENDIENTE** — Documentar cuadrícula, acciones, direcciones, pivotes, transparencia y orden de capas para cuerpo y cada objeto.
- **PENDIENTE** — Probar quieto, caminar, ataque con sable, ataque con lanza y uso de arco/bastón en el motor.
- **PENDIENTE** — Solo si las pruebas lo requieren, diseñar una extensión del paperdoll/animaciones; conservar compatibilidad con sprites estándar.

## Fase 7 — Multijugador, seguridad y operación

- **PENDIENTE** — Probar dos o más clientes simultáneos y validar movimiento, combate, inventario y desconexiones.
- **PENDIENTE** — Revisar autenticación, permisos del editor, validación de paquetes, abuso de habilidades y registros.
- **PENDIENTE** — Crear copias de seguridad y probar restauración de bases de datos.
- **PENDIENTE** — Validar puertos, cortafuegos, certificado HTTPS y configuración del host elegido.
- **PENDIENTE** — Medir carga y definir límites para mapas, entidades, efectos y conexiones.

## Fase 8 — Preparar versión de prueba y lanzamiento

- **PENDIENTE** — Resolver Release, ImageSharp y generación/gestión de claves de red.
- **PENDIENTE** — Revisar licencias del motor, bibliotecas, sonidos, música, fuentes, mapas y sprites.
- **PENDIENTE** — Crear paquetes limpios para cliente, editor y servidor con instrucciones de instalación y actualización.
- **PENDIENTE** — Hacer una prueba de instalación en una carpeta o equipo limpio y comprobar actualización y recuperación.
- **PENDIENTE** — Publicar una versión de prueba identificada y recoger errores de jugadores.

## Cómo actualizaremos el avance

- Cambiar una tarea a **COMPLETADO** solo cuando exista evidencia (compilación, prueba, captura, archivo o revisión documentada).
- Si una tarea necesita una regla que todavía no se decidió, marcarla **PENDIENTE** y registrar la decisión concreta.
- Al cerrar una fase, escribir fecha, commit y evidencia en esta hoja y en `ESTADO_PROYECTO.md`.
- Mantener los cambios en commits pequeños sobre `ultima-guerra`; no mezclar recursos con cambios de mecánicas sin una razón técnica documentada.

## Decisiones confirmadas

- Nombre: **El Legado de la Última Guerra**.
- Género: MMORPG 2D cenital de fantasía medieval.
- Atributos: Fuerza, Destreza, Inteligencia, Agilidad y Sabiduría.
- Prioridad: estabilidad, funcionamiento técnico, mecánicas y herramientas antes de mejoras gráficas.
- Los sprites estándar estilo RPG Maker XP sirvieron para la primera prueba técnica; los sprites LPC ampliados ya están guardados y se adaptarán después.
- Las animaciones de combate con brazos, armas y equipo en capas son un objetivo posterior.
