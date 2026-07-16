using EduApoyos.Application.Common;
using EduApoyos.Application.Features.Estudiantes.Create;
using EduApoyos.Application.Features.Estudiantes.List;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using FluentValidation;
using Moq;

namespace EduApoyos.Application.Tests;

/// <summary>
/// Pruebas unitarias de los handlers de estudiantes (creación y listado).
/// Valida persistencia, errores de usuario inexistente y mapeo de resultados paginados.
/// </summary>
public class EstudianteHandlerTests
{
    /// <summary>
    /// Valida que crear un estudiante persiste la entidad y retorna el DTO con el nombre del usuario asociado.
    /// </summary>
    [Fact]
    public async Task CreateHandler_PersisteEstudianteYRetornaDto()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Carlos Ruiz",
            Email = "carlos@example.com",
            PasswordHash = "hash",
            Rol = RolUsuario.Estudiante
        };

        var estudianteRepository = new Mock<IEstudianteRepository>();
        var usuarioRepository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        usuarioRepository
            .Setup(x => x.ObtenerPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var handler = new CreateEstudianteCommandHandler(
            estudianteRepository.Object,
            usuarioRepository.Object,
            unitOfWork.Object,
            new CreateEstudianteCommandValidator());

        var result = await handler.HandleAsync(new CreateEstudianteCommand(
            usuario.Id, "1000000002", TipoDocumento.CedulaCiudadania, "Medicina", 4));

        Assert.Equal("Carlos Ruiz", result.NombreCompleto);
        estudianteRepository.Verify(x => x.AgregarAsync(It.IsAny<Estudiante>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Valida que crear un estudiante con un usuario inexistente lanza <see cref="RecursoNoEncontradoException"/>.
    /// </summary>
    [Fact]
    public async Task CreateHandler_UsuarioInexistente_LanzaRecursoNoEncontrado()
    {
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var handler = new CreateEstudianteCommandHandler(
            new Mock<IEstudianteRepository>().Object,
            usuarioRepository.Object,
            new Mock<IUnitOfWork>().Object,
            new CreateEstudianteCommandValidator());

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => handler.HandleAsync(
            new CreateEstudianteCommand(Guid.NewGuid(), "1", TipoDocumento.Pasaporte, "Derecho", 2)));
    }

    /// <summary>
    /// Valida que el listado de estudiantes mapea correctamente un resultado paginado a DTOs.
    /// </summary>
    [Fact]
    public async Task ListHandler_MapeaResultadoPaginado()
    {
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Usuario = new Usuario { NombreCompleto = "Ana Díaz", Email = "ana@example.com" },
            NumeroDocumento = "1000000004",
            TipoDocumento = TipoDocumento.TarjetaIdentidad,
            ProgramaAcademico = "Psicología",
            Semestre = 3
        };

        var estudianteRepository = new Mock<IEstudianteRepository>();
        estudianteRepository
            .Setup(x => x.ListarAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Estudiante>([estudiante], 1, 10, 1));

        var handler = new ListEstudiantesQueryHandler(estudianteRepository.Object);
        var result = await handler.HandleAsync(new ListEstudiantesQuery(0, 500));

        Assert.Single(result.Items);
        Assert.Equal("Ana Díaz", result.Items.First().NombreCompleto);
    }
}
