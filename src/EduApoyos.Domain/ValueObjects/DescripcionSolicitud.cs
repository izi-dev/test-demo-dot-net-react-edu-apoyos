namespace EduApoyos.Domain.ValueObjects;

/// <summary>
/// Descripción o justificación de una solicitud de apoyo económico.
/// </summary>
/// <remarks>
/// Value object inmutable. Solo puede construirse mediante <see cref="Crear"/>,
/// que normaliza el texto y aplica las reglas de longitud y contenido mínimo.
/// </remarks>
public sealed record DescripcionSolicitud
{
    /// <summary>
    /// Texto de la descripción, ya normalizado (sin espacios al inicio ni al final).
    /// </summary>
    public string Valor { get; }

    /// <summary>
    /// Constructor privado que asigna el valor ya validado y normalizado.
    /// </summary>
    /// <param name="valor">Descripción previamente validada y recortada.</param>
    private DescripcionSolicitud(string valor) => Valor = valor;

    /// <summary>
    /// Crea una descripción validando longitud y contenido mínimo.
    /// </summary>
    /// <param name="valor">Texto de la descripción; se recorta con <c>Trim</c> antes de validar.</param>
    /// <returns>Una instancia de <see cref="DescripcionSolicitud"/> con el <see cref="Valor"/> normalizado.</returns>
    /// <exception cref="ArgumentException">
    /// Se lanza cuando el texto normalizado está vacío, o cuando supera los 1000 caracteres.
    /// </exception>
    /// <remarks>
    /// Validaciones y reglas aplicadas:
    /// <list type="bullet">
    /// <item><description>El valor de entrada se normaliza con <c>Trim</c>; si es <c>null</c>, se trata como cadena vacía.</description></item>
    /// <item><description>La descripción normalizada no puede tener longitud cero (es obligatoria).</description></item>
    /// <item><description>La descripción normalizada no puede superar 1000 caracteres.</description></item>
    /// <item><description>Si alguna regla falla, se lanza <see cref="ArgumentException"/> con el mensaje correspondiente.</description></item>
    /// </list>
    /// </remarks>
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
