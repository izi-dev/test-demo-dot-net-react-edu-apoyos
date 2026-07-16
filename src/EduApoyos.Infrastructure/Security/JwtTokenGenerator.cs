using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduApoyos.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Infrastructure.Security;

/// <summary>
/// Opciones de configuración para la emisión y validación de tokens JWT.
/// Se enlazan desde la sección <c>Jwt</c> de la configuración.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Emisor (<c>iss</c>) del token. Valor por defecto: <c>EduApoyos</c>.
    /// </summary>
    public string Issuer { get; set; } = "EduApoyos";

    /// <summary>
    /// Audiencia (<c>aud</c>) del token. Valor por defecto: <c>EduApoyos</c>.
    /// </summary>
    public string Audience { get; set; } = "EduApoyos";

    /// <summary>
    /// Clave secreta simétrica (HMAC-SHA256). Debe tener al menos 32 caracteres.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Minutos de vigencia del token desde su emisión. Valor por defecto: 60.
    /// </summary>
    public int ExpireMinutes { get; set; } = 60;
}

/// <summary>
/// Genera tokens JWT firmados a partir de un <see cref="UsuarioAutenticado"/>.
/// Incluye claims de sujeto, correo, identificador, nombre y rol.
/// </summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    /// <summary>
    /// Emite un token JWT para el usuario autenticado.
    /// </summary>
    /// <param name="usuario">Usuario ya autenticado del dominio de aplicación.</param>
    /// <returns>Token serializado y su fecha de expiración UTC.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si <see cref="JwtOptions.Key"/> está vacía o tiene menos de 32 caracteres.
    /// </exception>
    public TokenJwt Generar(UsuarioAutenticado usuario)
    {
        var jwt = options.Value;
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");
        }

        var expira = DateTime.UtcNow.AddMinutes(jwt.ExpireMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new TokenJwt(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
