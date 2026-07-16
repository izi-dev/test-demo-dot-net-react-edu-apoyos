using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de autenticación e identidad. Abstrae ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// Encapsula registro, validación de credenciales y sincronización con el usuario de dominio.
/// Los handlers de Auth dependen solo de este contrato, no de Identity directamente.
/// </remarks>
public interface IIdentityService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema de identidad y crea su proyección de dominio.
    /// </summary>
    /// <param name="solicitud">Datos de registro (nombre, email, contraseña y rol).</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>
    /// Resultado con <c>Exitoso = true</c> y el usuario autenticado si el registro fue válido;
    /// o <c>Exitoso = false</c> con el diccionario de errores de Identity.
    /// </returns>
    /// <remarks>
    /// No lanza excepciones de validación de Identity: las agrupa en
    /// <see cref="RegistroUsuarioResultado.Errores"/>. El handler de registro las traduce a
    /// <c>FluentValidation.ValidationException</c> si corresponde.
    /// </remarks>
    Task<RegistroUsuarioResultado> RegistrarAsync(RegistroUsuarioSolicitud solicitud, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida las credenciales de un usuario existente.
    /// </summary>
    /// <param name="email">Correo electrónico de acceso.</param>
    /// <param name="password">Contraseña en texto plano a verificar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>
    /// El usuario autenticado si las credenciales son correctas; de lo contrario <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Devuelve <c>null</c> tanto si el usuario no existe como si la contraseña es incorrecta,
    /// para no filtrar información de existencia de cuentas. El handler de login convierte
    /// ese <c>null</c> en <see cref="UnauthorizedAccessException"/>.
    /// </remarks>
    Task<UsuarioAutenticado?> AutenticarAsync(string email, string password, CancellationToken cancellationToken = default);
}

/// <summary>
/// Datos necesarios para registrar un usuario en el sistema.
/// </summary>
/// <param name="NombreCompleto">Nombre visible del usuario.</param>
/// <param name="Email">Correo único de acceso.</param>
/// <param name="Password">Contraseña en texto plano; Identity se encarga del hash.</param>
/// <param name="Rol">Rol de dominio a asignar (<see cref="RolUsuario"/>).</param>
public sealed record RegistroUsuarioSolicitud(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol);

/// <summary>
/// Resultado del proceso de registro de usuario.
/// </summary>
/// <param name="Exitoso">Indica si el registro se completó sin errores de Identity.</param>
/// <param name="Usuario">Usuario autenticado resultante; <c>null</c> si falló el registro.</param>
/// <param name="Errores">
/// Diccionario de errores por campo/clave de Identity; <c>null</c> o vacío cuando el registro es exitoso.
/// </param>
public sealed record RegistroUsuarioResultado(
    bool Exitoso,
    UsuarioAutenticado? Usuario,
    IReadOnlyDictionary<string, string[]>? Errores);

/// <summary>
/// Representa un usuario autenticado en el dominio de aplicación.
/// </summary>
/// <param name="Id">Identificador del usuario de dominio (alineado con Identity).</param>
/// <param name="NombreCompleto">Nombre completo del usuario.</param>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Rol">Rol de dominio asociado.</param>
public sealed record UsuarioAutenticado(
    Guid Id,
    string NombreCompleto,
    string Email,
    RolUsuario Rol);
