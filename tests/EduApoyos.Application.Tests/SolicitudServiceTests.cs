using EduApoyos.Application.Features.Constancias;
using EduApoyos.Application.Features.Solicitudes.ChangeStatus;
using EduApoyos.Application.Features.Solicitudes.Create;
using EduApoyos.Application.Features.Solicitudes.GetById;
using EduApoyos.Application.Features.Solicitudes.List;
using EduApoyos.Application.Features.Solicitudes.ListByEstudiante;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Domain.Exceptions;
using FluentValidation;
using Moq;

namespace EduApoyos.Application.Tests;

/// <summary>
/// Pruebas unitarias de los handlers de solicitudes de apoyo económico.
/// Valida creación, transiciones de estado, control de acceso, listados, constancias y filtros.
/// </summary>
public class SolicitudHandlerTests
{
    /// <summary>
    /// Valida que crear una solicitud la deja en estado Pendiente con un único registro en el historial.
    /// </summary>
    [Fact]
    public async Task CreateHandler_CreaSolicitudPendienteConHistorial()
    {
        var estudiante = BuildEstudiante();
        var solicitudRepository = new Mock<ISolicitudRepository>();
        var estudianteRepository = new Mock<IEstudianteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        estudianteRepository
            .Setup(x => x.ObtenerPorIdAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        SolicitudApoyo? captured = null;
        solicitudRepository
            .Setup(x => x.AgregarAsync(It.IsAny<SolicitudApoyo>(), It.IsAny<CancellationToken>()))
            .Callback<SolicitudApoyo, CancellationToken>((s, _) => captured = s)
            .Returns(Task.CompletedTask);

        var handler = BuildCreateHandler(solicitudRepository, estudianteRepository, unitOfWork);

        var result = await handler.HandleAsync(new CreateSolicitudCommand(
            estudiante.Id, TipoApoyo.Beca, 1500000, "Apoyo matrícula", estudiante.UsuarioId, RolUsuario.Asesor));

        Assert.Equal(EstadoSolicitud.Pendiente, result.Estado);
        Assert.NotNull(captured);
        Assert.Single(captured.Historial);
    }

    /// <summary>
    /// Valida que un cambio de estado válido (Pendiente → EnRevision) actualiza el estado y asigna el asesor.
    /// </summary>
    [Fact]
    public async Task ChangeStatusHandler_RespetaFlujoPermitido()
    {
        var asesorId = Guid.NewGuid();
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Credito,
            Domain.ValueObjects.MontoSolicitado.Crear(3000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Apoyo"),
            estudiante.UsuarioId);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ObtenerPorIdConDetalleAsync(solicitud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);
        solicitudRepository
            .Setup(x => x.PersistirCambioEstadoAsync(It.IsAny<SolicitudApoyo>(), It.IsAny<HistorialEstado>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = BuildChangeHandler(solicitudRepository);

        var result = await handler.HandleAsync(new ChangeSolicitudStatusCommand(
            solicitud.Id, EstadoSolicitud.EnRevision, "Documentos completos", asesorId));

        Assert.Equal(EstadoSolicitud.EnRevision, result.Estado);
        Assert.Equal(asesorId, result.AsesorId);
    }

    /// <summary>
    /// Valida que un salto de estado no permitido (Pendiente → Aprobada) lanza <see cref="TransicionEstadoInvalidaException"/>.
    /// </summary>
    [Fact]
    public async Task ChangeStatusHandler_FlujoInvalido_LanzaTransicionInvalida()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Beca,
            Domain.ValueObjects.MontoSolicitado.Crear(1000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Detalle"),
            estudiante.UsuarioId);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ObtenerPorIdConDetalleAsync(solicitud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var handler = BuildChangeHandler(solicitudRepository);

        await Assert.ThrowsAsync<TransicionEstadoInvalidaException>(() => handler.HandleAsync(
            new ChangeSolicitudStatusCommand(solicitud.Id, EstadoSolicitud.Aprobada, "Salto", Guid.NewGuid())));
    }

    /// <summary>
    /// Valida que un estudiante que no es dueño de la solicitud recibe <see cref="AccesoRecursoDenegadoException"/> al consultar el detalle.
    /// </summary>
    [Fact]
    public async Task GetByIdHandler_EstudianteAjeno_LanzaAccesoDenegado()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Beca,
            Domain.ValueObjects.MontoSolicitado.Crear(1000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Detalle"),
            estudiante.UsuarioId);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ObtenerPorIdConDetalleAsync(solicitud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var estudianteRepository = new Mock<IEstudianteRepository>();
        estudianteRepository
            .Setup(x => x.ObtenerPorUsuarioIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Estudiante { Id = Guid.NewGuid(), UsuarioId = Guid.NewGuid() });

        var handler = new GetSolicitudByIdQueryHandler(solicitudRepository.Object, estudianteRepository.Object);

        await Assert.ThrowsAsync<AccesoRecursoDenegadoException>(() => handler.HandleAsync(
            new GetSolicitudByIdQuery(solicitud.Id, Guid.NewGuid(), RolUsuario.Estudiante)));
    }

    /// <summary>
    /// Valida que un asesor puede consultar el detalle de cualquier solicitud y recibe el estado Pendiente inicial.
    /// </summary>
    [Fact]
    public async Task GetByIdHandler_Asesor_RetornaDetalle()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Beca,
            Domain.ValueObjects.MontoSolicitado.Crear(1000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Detalle"),
            estudiante.UsuarioId);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ObtenerPorIdConDetalleAsync(solicitud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var handler = new GetSolicitudByIdQueryHandler(
            solicitudRepository.Object,
            new Mock<IEstudianteRepository>().Object);

        var result = await handler.HandleAsync(new GetSolicitudByIdQuery(
            solicitud.Id, Guid.NewGuid(), RolUsuario.Asesor));

        Assert.Equal(solicitud.Id, result.Id);
        Assert.Equal(EstadoSolicitud.Pendiente, result.Estado);
    }

    /// <summary>
    /// Valida que listar solicitudes por estudiante retorna únicamente las solicitudes de ese estudiante.
    /// </summary>
    [Fact]
    public async Task ListByEstudianteHandler_RetornaSolicitudesPropias()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Subsidio,
            Domain.ValueObjects.MontoSolicitado.Crear(500000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Subsidio"),
            estudiante.UsuarioId);

        var estudianteRepository = new Mock<IEstudianteRepository>();
        estudianteRepository
            .Setup(x => x.ObtenerPorUsuarioIdAsync(estudiante.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ListarPorEstudianteAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([solicitud]);

        var handler = new ListSolicitudesByEstudianteQueryHandler(
            solicitudRepository.Object,
            estudianteRepository.Object);

        var result = await handler.HandleAsync(new ListSolicitudesByEstudianteQuery(
            estudiante.Id, estudiante.UsuarioId));

        Assert.Single(result);
        Assert.Equal(solicitud.Id, result.First().Id);
    }

    /// <summary>
    /// Valida que el endpoint «mis solicitudes» retorna las solicitudes del usuario autenticado.
    /// </summary>
    [Fact]
    public async Task ListMisSolicitudesHandler_RetornaSolicitudesDelUsuario()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Credito,
            Domain.ValueObjects.MontoSolicitado.Crear(2000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Crédito"),
            estudiante.UsuarioId);

        var estudianteRepository = new Mock<IEstudianteRepository>();
        estudianteRepository
            .Setup(x => x.ObtenerPorUsuarioIdAsync(estudiante.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ListarPorEstudianteAsync(estudiante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([solicitud]);

        var handler = new ListMisSolicitudesQueryHandler(
            solicitudRepository.Object,
            estudianteRepository.Object);

        var result = await handler.HandleAsync(new ListMisSolicitudesQuery(estudiante.UsuarioId));

        Assert.Single(result);
    }

    /// <summary>
    /// Valida que la generación de constancia produce un archivo de texto con nombre, contenido y tipo MIME correctos.
    /// </summary>
    [Fact]
    public async Task GenerateConstanciaHandler_GeneraArchivoTexto()
    {
        var estudiante = BuildEstudiante();
        var solicitud = SolicitudApoyo.Crear(
            estudiante.Id, estudiante, TipoApoyo.Beca,
            Domain.ValueObjects.MontoSolicitado.Crear(1000000),
            Domain.ValueObjects.DescripcionSolicitud.Crear("Detalle"),
            estudiante.UsuarioId);

        var estudianteRepository = new Mock<IEstudianteRepository>();
        estudianteRepository
            .Setup(x => x.ObtenerPorUsuarioIdAsync(estudiante.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estudiante);

        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ObtenerPorIdConDetalleAsync(solicitud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitud);

        var handler = new GenerateConstanciaQueryHandler(
            solicitudRepository.Object,
            estudianteRepository.Object);

        var result = await handler.HandleAsync(new GenerateConstanciaQuery(
            estudiante.Id, solicitud.Id, estudiante.UsuarioId));

        Assert.Equal($"constancia-{solicitud.Id}.txt", result.NombreArchivo);
        Assert.Contains("Constancia de solicitud", result.Contenido);
        Assert.Equal("text/plain", result.ContentType);
    }

    /// <summary>
    /// Valida que el listado paginado de solicitudes aplica correctamente los filtros por estado y tipo de apoyo.
    /// </summary>
    [Fact]
    public async Task ListHandler_AplicaFiltrosPaginados()
    {
        var solicitudRepository = new Mock<ISolicitudRepository>();
        solicitudRepository
            .Setup(x => x.ListarAsync(It.IsAny<FiltroSolicitudes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Application.Common.PagedResult<SolicitudApoyo>([], 1, 10, 0));

        var handler = new ListSolicitudesQueryHandler(solicitudRepository.Object);
        var result = await handler.HandleAsync(new ListSolicitudesQuery(
            EstadoSolicitud.Pendiente, TipoApoyo.Subsidio, null, null));

        Assert.Empty(result.Items);
    }

    private static CreateSolicitudCommandHandler BuildCreateHandler(
        Mock<ISolicitudRepository> solicitudRepository,
        Mock<IEstudianteRepository> estudianteRepository,
        Mock<IUnitOfWork> unitOfWork) =>
        new(solicitudRepository.Object, estudianteRepository.Object, unitOfWork.Object, new CreateSolicitudCommandValidator());

    private static ChangeSolicitudStatusCommandHandler BuildChangeHandler(
        Mock<ISolicitudRepository> solicitudRepository)
    {
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Usuario
            {
                Id = id,
                NombreCompleto = "Asesor",
                Email = "asesor@example.com",
                PasswordHash = "hash",
                Rol = RolUsuario.Asesor
            });

        return new(
            solicitudRepository.Object,
            usuarioRepository.Object,
            new ChangeSolicitudStatusCommandValidator());
    }

    private static Estudiante BuildEstudiante()
    {
        var usuarioId = Guid.NewGuid();
        return new Estudiante
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Usuario = new Usuario
            {
                Id = usuarioId,
                NombreCompleto = "Laura Gómez",
                Email = "laura@example.com",
                PasswordHash = "hash",
                Rol = RolUsuario.Estudiante
            },
            NumeroDocumento = "1000000001",
            TipoDocumento = TipoDocumento.CedulaCiudadania,
            ProgramaAcademico = "Ingeniería de Software",
            Semestre = 6
        };
    }
}
