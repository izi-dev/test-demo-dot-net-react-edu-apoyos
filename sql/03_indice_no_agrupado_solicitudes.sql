-- Ejercicio 3
-- Índice no agrupado sobre la tabla de solicitudes.
--
-- Justificación:
-- El panel del asesor (endpoint GET /api/solicitudes) filtra frecuentemente por
-- "Estado" y "TipoApoyo", y ordena/consulta por fecha. El ejercicio 1 además filtra
-- por "Estado" y "FechaActualizacion". Un índice compuesto sobre estas columnas evita
-- recorridos completos de tabla (Seq Scan) y acelera los filtros combinados más comunes.
--
-- En PostgreSQL todos los índices creados con CREATE INDEX son "no agrupados"
-- (el equivalente al índice clustered de SQL Server es la organización física de la tabla,
-- que aquí no se altera).

CREATE INDEX IF NOT EXISTS "IX_SolicitudesApoyo_Estado_Tipo_FechaActualizacion"
    ON "SolicitudesApoyo" ("Estado", "TipoApoyo", "FechaActualizacion");

-- Verificación del plan de ejecución esperado:
-- EXPLAIN ANALYZE
-- SELECT * FROM "SolicitudesApoyo"
-- WHERE "Estado" = 'Pendiente' AND "TipoApoyo" = 'Beca';
