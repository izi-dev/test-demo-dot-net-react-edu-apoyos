namespace EduApoyos.Domain.ValueObjects;

/// <summary>
/// Representa un monto económico válido para una solicitud de apoyo.
/// </summary>
/// <remarks>
/// Value object inmutable. Solo puede construirse mediante <see cref="Crear"/>,
/// que aplica las reglas de negocio institucionales sobre el importe.
/// </remarks>
public sealed record MontoSolicitado
{
    /// <summary>
    /// Valor numérico del monto solicitado.
    /// </summary>
    public decimal Valor { get; }

    /// <summary>
    /// Constructor privado que asigna el valor ya validado.
    /// </summary>
    /// <param name="valor">Monto económico previamente validado.</param>
    private MontoSolicitado(decimal valor) => Valor = valor;

    /// <summary>
    /// Crea un monto validando que sea positivo y no supere el límite institucional.
    /// </summary>
    /// <param name="valor">Importe a validar; debe ser mayor que cero y no superar 100.000.000.</param>
    /// <returns>Una instancia de <see cref="MontoSolicitado"/> con el <see cref="Valor"/> indicado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se lanza cuando <paramref name="valor"/> es menor o igual a cero, o cuando supera el límite de 100.000.000.
    /// </exception>
    /// <remarks>
    /// Validaciones y reglas aplicadas:
    /// <list type="bullet">
    /// <item><description><paramref name="valor"/> debe ser estrictamente mayor que cero (<c>valor &gt; 0</c>).</description></item>
    /// <item><description><paramref name="valor"/> no puede superar el límite institucional de 100.000.000.</description></item>
    /// <item><description>Si alguna regla falla, se lanza <see cref="ArgumentOutOfRangeException"/> con el mensaje correspondiente.</description></item>
    /// </list>
    /// </remarks>
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
