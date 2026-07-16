-- Ejercicio 2
-- Cuenta el total de solicitudes agrupadas por estado y tipo de apoyo
-- registradas durante el último mes (últimos 30 días).
SELECT
    s."Estado",
    s."TipoApoyo",
    COUNT(*) AS total_solicitudes
FROM "SolicitudesApoyo" s
WHERE s."FechaSolicitud" >= NOW() - INTERVAL '1 month'
GROUP BY s."Estado", s."TipoApoyo"
ORDER BY s."Estado", s."TipoApoyo";
