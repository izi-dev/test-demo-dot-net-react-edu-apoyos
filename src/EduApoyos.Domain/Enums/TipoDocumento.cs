namespace EduApoyos.Domain.Enums;

/// <summary>
/// Tipos de documento de identificación válidos para registrar estudiantes.
/// </summary>
public enum TipoDocumento
{
    /// <summary>
    /// Cédula de ciudadanía colombiana.
    /// </summary>
    CedulaCiudadania = 1,

    /// <summary>
    /// Tarjeta de identidad para menores de edad.
    /// </summary>
    TarjetaIdentidad = 2,

    /// <summary>
    /// Cédula de extranjería para residentes no nacionales.
    /// </summary>
    CedulaExtranjeria = 3,

    /// <summary>
    /// Pasaporte internacional.
    /// </summary>
    Pasaporte = 4
}
