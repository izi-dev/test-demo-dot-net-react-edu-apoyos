using EduApoyos.Application.Features.Auth;
using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Application.Features.Auth.Register;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;
using FluentValidation;
using Moq;

namespace EduApoyos.Application.Tests;

public class AuthHandlerTests
{
    [Fact]
    public async Task RegisterHandler_UsuarioValido_RetornaToken()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new UsuarioAutenticado(usuarioId, "María López", "maria@example.com", RolUsuario.Estudiante);
        var expira = DateTime.UtcNow.AddHours(1);

        var identityService = new Mock<IIdentityService>();
        identityService
            .Setup(x => x.RegistrarAsync(It.IsAny<RegistroUsuarioSolicitud>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegistroUsuarioResultado(true, usuario, null));

        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        jwtGenerator
            .Setup(x => x.Generar(usuario))
            .Returns(new TokenJwt("jwt-token", expira));

        var handler = new RegisterCommandHandler(
            identityService.Object,
            jwtGenerator.Object,
            new RegisterCommandValidator());

        var result = await handler.HandleAsync(new RegisterCommand(
            "María López", "maria@example.com", "Password123", RolUsuario.Estudiante));

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(usuarioId, result.UserId);
        Assert.Equal(RolUsuario.Estudiante, result.Rol);
    }

    [Fact]
    public async Task RegisterHandler_IdentityFalla_LanzaValidationException()
    {
        var identityService = new Mock<IIdentityService>();
        identityService
            .Setup(x => x.RegistrarAsync(It.IsAny<RegistroUsuarioSolicitud>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegistroUsuarioResultado(
                false,
                null,
                new Dictionary<string, string[]> { ["Email"] = ["El correo ya está registrado."] }));

        var handler = new RegisterCommandHandler(
            identityService.Object,
            new Mock<IJwtTokenGenerator>().Object,
            new RegisterCommandValidator());

        await Assert.ThrowsAsync<ValidationException>(() => handler.HandleAsync(new RegisterCommand(
            "María López", "maria@example.com", "Password123", RolUsuario.Estudiante)));
    }

    [Fact]
    public async Task LoginHandler_CredencialesValidas_RetornaToken()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new UsuarioAutenticado(usuarioId, "Pedro Ruiz", "pedro@example.com", RolUsuario.Asesor);
        var expira = DateTime.UtcNow.AddHours(1);

        var identityService = new Mock<IIdentityService>();
        identityService
            .Setup(x => x.AutenticarAsync("pedro@example.com", "Asesor123*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        jwtGenerator
            .Setup(x => x.Generar(usuario))
            .Returns(new TokenJwt("jwt-login", expira));

        var handler = new LoginCommandHandler(
            identityService.Object,
            jwtGenerator.Object,
            new LoginCommandValidator());

        var result = await handler.HandleAsync(new LoginCommand("pedro@example.com", "Asesor123*"));

        Assert.Equal("jwt-login", result.Token);
        Assert.Equal(RolUsuario.Asesor, result.Rol);
    }

    [Fact]
    public async Task LoginHandler_CredencialesInvalidas_LanzaUnauthorized()
    {
        var identityService = new Mock<IIdentityService>();
        identityService
            .Setup(x => x.AutenticarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioAutenticado?)null);

        var handler = new LoginCommandHandler(
            identityService.Object,
            new Mock<IJwtTokenGenerator>().Object,
            new LoginCommandValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new LoginCommand("inexistente@example.com", "wrong")));
    }
}
