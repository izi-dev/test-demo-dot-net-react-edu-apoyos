namespace EduApoyos.Domain.ValueObjects;

/// <summary>
/// Representa un monto económico válido para una solicitud de apoyo.
/// </summary>
public sealed record MontoSolicitado
{
    public decimal Valor { get; }

    private MontoSolicitado(decimal valor) => Valor = valor;

    /// <summary>
    /// Crea un monto validando que sea positivo y no supere el límite institucional.
    /// </summary>
    public static MontoSolicitado Crear(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "El monto debe ser mayor que cero.");
        }

        if (valor > 100_000_000)
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "El monto supera el límite permitido.");
        }

        return new MontoSolicitado(valor);
    }
}
