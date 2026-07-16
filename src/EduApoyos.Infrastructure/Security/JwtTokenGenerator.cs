using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduApoyos.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Infrastructure.Security;

/// <summary>
/// Opciones de configuración para la emisión de tokens JWT.
/// </summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "EduApoyos";
    public string Audience { get; set; } = "EduApoyos";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 60;
}

/// <summary>
/// Genera tokens JWT a partir de un usuario autenticado del dominio de aplicación.
/// </summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
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
