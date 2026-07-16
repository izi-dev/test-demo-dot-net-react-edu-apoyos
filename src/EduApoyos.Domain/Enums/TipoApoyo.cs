namespace EduApoyos.Domain.Enums;

/// <summary>
/// Tipos de apoyo económico que un estudiante puede solicitar.
/// </summary>
public enum TipoApoyo
{
    /// <summary>
    /// Apoyo otorgado como beca, sin obligación de devolución.
    /// </summary>
    Beca = 1,

    /// <summary>
    /// Apoyo otorgado como crédito educativo con condiciones de devolución.
    /// </summary>
    Credito = 2,

    /// <summary>
    /// Apoyo otorgado como subsidio parcial para gastos académicos.
    /// </summary>
    Subsidio = 3
}
