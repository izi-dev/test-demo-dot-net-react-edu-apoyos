using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.ValueObjects;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Servicio que inserta datos iniciales de demostración cuando la base de datos está vacía.
/// </summary>
public sealed class DataSeeder(
    EduApoyosDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    /// <summary>
    /// Garantiza la existencia de roles, usuarios de prueba, un estudiante y una solicitud de ejemplo.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
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

    private async Task EnsureRoleAsync(RolUsuario rol)
    {
        var roleName = rol.ToString();

        if (!await roleManager.RoleExistsAsync(roleName: roleName))
        {
            await roleManager.CreateAsync(role: new IdentityRole<Guid>(roleName));
        }
    }

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
