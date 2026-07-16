namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto para generar tokens JWT sin acoplar la aplicación a la infraestructura de seguridad.
/// </summary>
public interface IJwtTokenGenerator
{
    TokenJwt Generar(UsuarioAutenticado usuario);
}

/// <summary>
/// Token JWT emitido tras autenticación o registro exitoso.
/// </summary>
public sealed record TokenJwt(string Valor, DateTime ExpiraEn);
