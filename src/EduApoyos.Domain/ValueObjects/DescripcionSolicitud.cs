namespace EduApoyos.Domain.ValueObjects;

/// <summary>
/// Descripción o justificación de una solicitud de apoyo económico.
/// </summary>
public sealed record DescripcionSolicitud
{
    public string Valor { get; }

    private DescripcionSolicitud(string valor) => Valor = valor;

    /// <summary>
    /// Crea una descripción validando longitud y contenido mínimo.
    /// </summary>
    public static DescripcionSolicitud Crear(string valor)
    {
        var normalizada = valor?.Trim() ?? string.Empty;

        if (normalizada.Length == 0)
        {
            throw new ArgumentException("La descripción es obligatoria.", nameof(valor));
        }

        if (normalizada.Length > 1000)
        {
            throw new ArgumentException("La descripción no puede superar 1000 caracteres.", nameof(valor));
        }

        return new DescripcionSolicitud(normalizada);
    }
}
