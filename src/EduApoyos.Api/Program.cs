using System.Text;
using System.Text.Json.Serialization;
using EduApoyos.Api.Middleware;
using EduApoyos.Application;
using EduApoyos.Infrastructure;
using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

/*
 * Serialización JSON de enums:
 * JsonStringEnumConverter escribe y lee los valores enum como texto
 * (p. ej. "Asesor", "Pendiente", "Beca") en lugar de enteros.
 * Así el contrato HTTP coincide con lo que espera el frontend.
 */
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

/*
 * CORS (política "Frontend"):
 * Solo se permiten orígenes listados en Cors:AllowedOrigins
 * (por defecto http://localhost:5173 para Vite).
 * AllowAnyHeader / AllowAnyMethod habilitan preflight para la SPA.
 * No se usan credenciales de cookie; la API autentica con Bearer JWT.
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

/*
 * Autenticación JWT Bearer:
 * - Esquema por defecto: JwtBearer (sin cookies de Identity).
 * - Valida emisor (iss), audiencia (aud), tiempo de vida y firma HMAC-SHA256.
 * - Issuer, Audience y Key se leen de la sección Jwt de la configuración
 *   (mismas opciones que usa JwtTokenGenerator al emitir tokens).
 * - Los roles viajan en el claim ClaimTypes.Role y alimentan [Authorize(Roles = ...)].
 */
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "EduApoyos API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.MigrateAndSeedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
/// Clase parcial de entrada para pruebas de integración (WebApplicationFactory).
/// </summary>
public partial class Program;
