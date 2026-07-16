using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// Adaptador de ASP.NET Core Identity hacia el puerto <see cref="IIdentityService"/>.
/// Crea usuarios en Identity y sincroniza la entidad de dominio <see cref="Usuario"/>.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUsuarioRepository usuarios,
    IUnitOfWork unitOfWork) : IIdentityService
{
    /// <summary>
    /// Registra un usuario en Identity, lo asigna a su rol y persiste el usuario de dominio.
    /// </summary>
    /// <param name="solicitud">Datos de registro (nombre, correo, contraseña y rol).</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>
    /// Resultado con el usuario autenticado mapeado si tuvo éxito,
    /// o un diccionario de errores de Identity si falló.
    /// </returns>
    public async Task<RegistroUsuarioResultado> RegistrarAsync(
        RegistroUsuarioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Rol = solicitud.Rol,
            FechaRegistro = DateTime.UtcNow
        };

        var resultado = await userManager.CreateAsync(user, solicitud.Password);
        if (!resultado.Succeeded)
        {
            return new RegistroUsuarioResultado(
                Exitoso: false,
                Usuario: null,
                Errores: resultado.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));
        }

        await userManager.AddToRoleAsync(user, solicitud.Rol.ToString());

        await usuarios.AgregarAsync(new Usuario
        {
            Id = user.Id,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email!,
            PasswordHash = user.PasswordHash ?? string.Empty,
            Rol = user.Rol,
            FechaRegistro = user.FechaRegistro
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegistroUsuarioResultado(
            Exitoso: true,
            Usuario: Map(user),
            Errores: null);
    }

    /// <summary>
    /// Valida correo y contraseña contra Identity.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña en texto plano.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>
    /// Usuario autenticado mapeado si las credenciales son válidas; de lo contrario, <c>null</c>.
    /// </returns>
    public async Task<UsuarioAutenticado?> AutenticarAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return Map(user);
    }

    /// <summary>
    /// Proyecta un <see cref="ApplicationUser"/> al DTO de aplicación <see cref="UsuarioAutenticado"/>.
    /// </summary>
    /// <param name="user">Usuario de Identity.</param>
    /// <returns>Representación autenticada usada por la capa de aplicación.</returns>
    private static UsuarioAutenticado Map(ApplicationUser user) =>
        new(user.Id, user.NombreCompleto, user.Email!, user.Rol);
}
