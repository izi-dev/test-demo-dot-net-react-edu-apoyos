using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// Contexto de Entity Framework Core para EduApoyos.
/// Extiende Identity e implementa <see cref="IUnitOfWork"/> para coordinar transacciones.
/// </summary>
public sealed class EduApoyosDbContext(DbContextOptions<EduApoyosDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    /// <summary>
    /// Conjunto de usuarios del dominio (espejo de Identity).
    /// </summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>
    /// Conjunto de perfiles de estudiante vinculados a un usuario.
    /// </summary>
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    /// <summary>
    /// Conjunto de solicitudes de apoyo económico.
    /// </summary>
    public DbSet<SolicitudApoyo> SolicitudesApoyo => Set<SolicitudApoyo>();

    /// <summary>
    /// Conjunto de registros del historial de cambios de estado de solicitudes.
    /// </summary>
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    /// <summary>
    /// Configura tablas, conversiones de enums, índices y relaciones del modelo.
    /// </summary>
    /// <param name="builder">Constructor del modelo de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder: builder);

        builder.Entity<ApplicationUser>(buildAction: entity =>
        {
            entity.ToTable(name: "AspNetUsers");
            entity.Property(propertyExpression: x => x.NombreCompleto).HasMaxLength(maxLength: 160).IsRequired();
            entity.Property(propertyExpression: x => x.Rol).HasConversion<string>().HasMaxLength(maxLength: 30).IsRequired();
        });

        builder.Entity<Usuario>(buildAction: entity =>
        {
            entity.ToTable(name: "Usuarios");
            entity.HasKey(keyExpression: x => x.Id);
            entity.Property(propertyExpression: x => x.NombreCompleto).HasMaxLength(maxLength: 160).IsRequired();
            entity.Property(propertyExpression: x => x.Email).HasMaxLength(maxLength: 256).IsRequired();
            entity.Property(propertyExpression: x => x.PasswordHash).HasMaxLength(maxLength: 500).IsRequired();
            entity.Property(propertyExpression: x => x.Rol).HasConversion<string>().HasMaxLength(maxLength: 30).IsRequired();
            entity.HasIndex(indexExpression: x => x.Email).IsUnique();
        });

        builder.Entity<Estudiante>(buildAction: entity =>
        {
            entity.ToTable(name: "Estudiantes");
            entity.HasKey(keyExpression: x => x.Id);
            entity.Property(propertyExpression: x => x.NumeroDocumento).HasMaxLength(maxLength: 30).IsRequired();
            entity.Property(propertyExpression: x => x.TipoDocumento).HasConversion<string>().HasMaxLength(maxLength: 40).IsRequired();
            entity.Property(propertyExpression: x => x.ProgramaAcademico).HasMaxLength(maxLength: 160).IsRequired();
            entity.HasIndex(indexExpression: x => x.NumeroDocumento).IsUnique();

            entity.HasOne(navigationExpression: x => x.Usuario)
                .WithMany()
                .HasForeignKey(foreignKeyExpression: x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SolicitudApoyo>(buildAction: entity =>
        {
            entity.ToTable(name: "SolicitudesApoyo");
            entity.HasKey(keyExpression: x => x.Id);
            entity.Property(propertyExpression: x => x.TipoApoyo).HasConversion<string>().HasMaxLength(maxLength: 30).IsRequired();
            entity.Property(propertyExpression: x => x.Estado).HasConversion<string>().HasMaxLength(maxLength: 30).IsRequired();
            entity.Property(propertyExpression: x => x.MontoSolicitado).HasPrecision(precision: 18, scale: 2);
            entity.Property(propertyExpression: x => x.Descripcion).HasMaxLength(maxLength: 1000).IsRequired();

            entity.HasIndex(indexExpression: x => new { x.Estado, x.TipoApoyo, x.FechaActualizacion })
                .HasDatabaseName(name: "IX_SolicitudesApoyo_Estado_Tipo_FechaActualizacion");

            entity.HasOne(navigationExpression: x => x.Estudiante)
                .WithMany(x => x.Solicitudes)
                .HasForeignKey(foreignKeyExpression: x => x.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(navigationExpression: x => x.Asesor)
                .WithMany()
                .HasForeignKey(foreignKeyExpression: x => x.AsesorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<HistorialEstado>(buildAction: entity =>
        {
            entity.ToTable(name: "HistorialEstados");
            entity.HasKey(keyExpression: x => x.Id);
            entity.Property(propertyExpression: x => x.EstadoAnterior).HasConversion<string>().HasMaxLength(maxLength: 30);
            entity.Property(propertyExpression: x => x.EstadoNuevo).HasConversion<string>().HasMaxLength(maxLength: 30).IsRequired();
            entity.Property(propertyExpression: x => x.Observacion).HasMaxLength(maxLength: 500).IsRequired();

            entity.HasOne(navigationExpression: x => x.Solicitud)
                .WithMany(x => x.Historial)
                .HasForeignKey(foreignKeyExpression: x => x.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(navigationExpression: x => x.Usuario)
                .WithMany()
                .HasForeignKey(foreignKeyExpression: x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
