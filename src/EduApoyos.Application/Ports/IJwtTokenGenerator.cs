namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto para generar tokens JWT sin acoplar la aplicación a la infraestructura de seguridad.
/// </summary>
/// <remarks>
/// Los handlers de autenticación usan este puerto tras un login o registro exitoso
/// para emitir el token que consume la API.
/// </remarks>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un token JWT firmado para el usuario autenticado indicado.
    /// </summary>
    /// <param name="usuario">Datos del usuario autenticado que se incluirán en las claims.</param>
    /// <returns>Token JWT con su valor serializado y la fecha/hora de expiración.</returns>
    /// <remarks>
    /// La implementación concreta define algoritmo, claves, claims (id, email, rol, etc.)
    /// y política de expiración. Este método es síncrono porque solo firma en memoria.
    /// </remarks>
    TokenJwt Generar(UsuarioAutenticado usuario);
}

/// <summary>
/// Token JWT emitido tras autenticación o registro exitoso.
/// </summary>
/// <param name="Valor">Cadena compacta del JWT (header.payload.signature).</param>
/// <param name="ExpiraEn">Fecha/hora UTC (o del reloj de la app) en la que el token deja de ser válido.</param>
public sealed record TokenJwt(string Valor, DateTime ExpiraEn);
