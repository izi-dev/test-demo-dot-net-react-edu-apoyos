-- Ejercicio 1
-- Lista las solicitudes en estado 'Pendiente' con más de 5 días sin actualización,
-- ordenadas de la más antigua a la más reciente (por antigüedad de la última actualización).
SELECT
    s."Id",
    e."NumeroDocumento",
    u."NombreCompleto"                                   AS estudiante,
    s."TipoApoyo",
    s."MontoSolicitado",
    s."FechaSolicitud",
    s."FechaActualizacion",
    (NOW() - s."FechaActualizacion")                     AS tiempo_sin_actualizar
FROM "SolicitudesApoyo" s
INNER JOIN "Estudiantes" e ON e."Id" = s."EstudianteId"
INNER JOIN "Usuarios"    u ON u."Id" = e."UsuarioId"
WHERE s."Estado" = 'Pendiente'
  AND s."FechaActualizacion" < NOW() - INTERVAL '5 days'
ORDER BY s."FechaActualizacion" ASC;
