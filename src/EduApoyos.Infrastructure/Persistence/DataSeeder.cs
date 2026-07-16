using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.ValueObjects;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Inserta datos iniciales de demostración (roles, usuarios, estudiante y solicitud)
/// cuando aún no existen en la base de datos.
/// </summary>
public sealed class DataSeeder(
    EduApoyosDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    /// <summary>
    /// Garantiza roles, usuarios de prueba, un estudiante asociado y una solicitud de ejemplo.
    /// Es idempotente: no duplica datos si ya existen.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando el seed ha finalizado.</returns>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync(rol: RolUsuario.Asesor);
        await EnsureRoleAsync(rol: RolUsuario.Estudiante);

        var asesor = await EnsureUserAsync(
            email: "asesor@educapoyos.local",
            nombre: "Asesor Principal",
            rol: RolUsuario.Asesor,
            password: "Asesor123*");

        var estudianteUser = await EnsureUserAsync(
            email: "estudiante@educapoyos.local",
            nombre: "Laura Gómez",
            rol: RolUsuario.Estudiante,
            password: "Estudiante123*");

        var usuarioEstudiante = context.Usuarios.Local
            .FirstOrDefault(x => x.Id == estudianteUser.Id)
            ?? await context.Usuarios.FirstAsync(
                x => x.Id == estudianteUser.Id,
                cancellationToken);

        var estudiante = await context.Estudiantes
            .Include(navigationPropertyPath: x => x.Usuario)
            .FirstOrDefaultAsync(
                predicate: x => x.UsuarioId == estudianteUser.Id,
                cancellationToken: cancellationToken);

        if (estudiante is null)
        {
            estudiante = Estudiante.Crear(
                usuarioId: estudianteUser.Id,
                usuario: usuarioEstudiante,
                numeroDocumento: "1000000001",
                tipoDocumento: TipoDocumento.CedulaCiudadania,
                programaAcademico: "Ingeniería de Software",
                semestre: 6);

            await context.Estudiantes.AddAsync(estudiante, cancellationToken);
        }

        if (!await context.SolicitudesApoyo.AnyAsync(cancellationToken: cancellationToken))
        {
            var solicitud = SolicitudApoyo.Crear(
                estudianteId: estudiante.Id,
                estudiante: estudiante,
                tipoApoyo: TipoApoyo.Beca,
                monto: MontoSolicitado.Crear(2500000),
                descripcion: DescripcionSolicitud.Crear("Solicitud de apoyo económico para matrícula."),
                usuarioCreadorId: estudianteUser.Id);

            await context.SolicitudesApoyo.AddAsync(solicitud, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Crea el rol de Identity si aún no existe.
    /// </summary>
    /// <param name="rol">Rol de dominio a materializar como <see cref="IdentityRole{TKey}"/>.</param>
    private async Task EnsureRoleAsync(RolUsuario rol)
    {
        var roleName = rol.ToString();

        if (!await roleManager.RoleExistsAsync(roleName: roleName))
        {
            await roleManager.CreateAsync(role: new IdentityRole<Guid>(roleName));
        }
    }

    /// <summary>
    /// Obtiene o crea un usuario de Identity y sincroniza la entidad de dominio <see cref="Usuario"/>.
    /// </summary>
    /// <param name="email">Correo del usuario de demostración.</param>
    /// <param name="nombre">Nombre completo.</param>
    /// <param name="rol">Rol de negocio.</param>
    /// <param name="password">Contraseña inicial.</param>
    /// <returns>El usuario de Identity existente o recién creado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si Identity no puede crear el usuario (errores de validación agregados en el mensaje).
    /// </exception>
    private async Task<ApplicationUser> EnsureUserAsync(
        string email,
        string nombre,
        RolUsuario rol,
        string password)
    {
        var existing = await userManager.FindByEmailAsync(email: email);

        if (existing is not null)
        {
            await EnsureDomainUserAsync(user: existing);

            return existing;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = nombre,
            Rol = rol,
            FechaRegistro = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(
            user: user,
            password: password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                message: string.Join(
                    separator: "; ",
                    values: result.Errors.Select(selector: x => x.Description)));
        }

        await userManager.AddToRoleAsync(
            user: user,
            role: rol.ToString());

        await EnsureDomainUserAsync(user: user);

        return user;
    }

    /// <summary>
    /// Inserta el espejo de dominio <see cref="Usuario"/> si aún no existe para el Identity user.
    /// </summary>
    /// <param name="user">Usuario de Identity ya persistido.</param>
    private async Task EnsureDomainUserAsync(ApplicationUser user)
    {
        if (await context.Usuarios.AnyAsync(
                predicate: x => x.Id == user.Id,
                cancellationToken: default))
        {
            return;
        }

        await context.Usuarios.AddAsync(entity: new Usuario
        {
            Id = user.Id,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email!,
            PasswordHash = user.PasswordHash ?? string.Empty,
            Rol = user.Rol,
            FechaRegistro = user.FechaRegistro
        });
    }
}
