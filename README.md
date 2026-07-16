# EduApoyos

Sistema web para gestionar solicitudes de apoyo económico (becas, créditos y subsidios) de estudiantes de educación superior. Los **asesores** registran y gestionan solicitudes y estudiantes; los **estudiantes** consultan el estado de sus solicitudes y descargan una constancia desde un portal de autogestión.

Monorepo con **.NET 8** (Clean Architecture) y **React + Vite**, con Docker Compose para ejecución local y PostgreSQL.

## Demo en línea

Aplicación desplegada:

- **Frontend:** [https://fvargas.dezeo.cloud/](https://fvargas.dezeo.cloud/)
- **Sobre el autor:** [https://fvargas.dezeo.cloud/login#sobre-mi](https://fvargas.dezeo.cloud/login#sobre-mi)
- **Swagger (API):** [https://fvargas.dezeo.cloud/swagger/index.html](https://fvargas.dezeo.cloud/swagger/index.html)
- **API (login):** `https://fvargas.dezeo.cloud/api/auth/login`

Credenciales de prueba (mismas del seed):

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Asesor | `asesor@educapoyos.local` | `Asesor123*` |
| Estudiante | `estudiante@educapoyos.local` | `Estudiante123*` |

## Stack

| Componente | Tecnología |
|------------|------------|
| Backend | .NET 8, ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (Code First) |
| Base de datos | PostgreSQL (Npgsql) |
| Autenticación | ASP.NET Core Identity + JWT Bearer |
| Frontend | React, Vite, TypeScript, React Router, Axios |
| Documentación API | Swagger / OpenAPI (Swashbuckle) |
| Pruebas | xUnit + Moq + Coverlet |
| Contenedores | Docker, Docker Compose, Nginx |
| CI | GitHub Actions (backend y frontend) |

> Nota sobre la base de datos: el enunciado sugiere SQL Server. Se eligió **PostgreSQL** por disponibilidad local y despliegue con contenedores. Con EF Core el cambio de proveedor implica sustituir `UseNpgsql` por `UseSqlServer` y regenerar migraciones.

## Arquitectura

Separación en cuatro capas con dependencias hacia el dominio. La capa **Application** está organizada por **vertical slices** (features): cada caso de uso tiene su comando/consulta, handler y validador, sin servicios monolíticos.

```
src/
├── EduApoyos.Domain/          # Agregados, value objects, excepciones de dominio.
├── EduApoyos.Application/     # Features (handlers), puertos (interfaces), contratos CQRS.
│   ├── Features/              # Auth, Estudiantes, Solicitudes, Constancias
│   ├── Ports/                 # IUnitOfWork, repositorios, IIdentityService, IJwtTokenGenerator
│   └── Common/Abstractions/   # ICommandHandler, IQueryHandler
├── EduApoyos.Infrastructure/  # EF Core, Identity, JWT, implementación de puertos.
└── EduApoyos.Api/             # Controladores delgados, middleware, Swagger.
tests/
└── EduApoyos.Application.Tests/
frontend/                      # SPA React (Vite)
sql/                           # Scripts SQL requeridos
```

### Principios SOLID aplicados

| Principio | Cómo se aplica |
|-----------|----------------|
| **S** (Single Responsibility) | Cada handler implementa un solo caso de uso. Los controladores solo traducen HTTP ↔ comandos/consultas. |
| **O** (Open/Closed) | Nuevos casos de uso = nueva carpeta en `Features/` sin modificar handlers existentes. |
| **L** (Liskov) | Los repositorios e `IIdentityService` son sustituibles en pruebas con `Moq`. |
| **I** (Interface Segregation) | Puertos pequeños y orientados al caso de uso (`ISolicitudRepository`, `IJwtTokenGenerator`, etc.). |
| **D** (Dependency Inversion) | Application define interfaces; Infrastructure las implementa. La API no referencia Identity directamente. |

### Patrones de diseño

1. **Repository** (`IEstudianteRepository`, `ISolicitudRepository`, `IUsuarioRepository`): abstrae persistencia detrás de puertos en Application.
2. **CQRS por handlers**: comandos (`CreateSolicitudCommandHandler`) y consultas (`ListSolicitudesQueryHandler`) separados, sin MediatR para mantener el alcance acotado.
3. **Unit of Work** (`IUnitOfWork`, implementado por `EduApoyosDbContext`) para confirmar cambios transaccionales.
4. **Dominio rico**: `SolicitudApoyo` encapsula transiciones de estado; value objects (`MontoSolicitado`, `DescripcionSolicitud`) validan invariantes.
5. **Puertos y adaptadores**: Identity y JWT viven en Infrastructure (`IdentityService`, `JwtTokenGenerator`); la API solo conoce handlers.

### Flujo de estados de una solicitud

La transición vive en el agregado (`SolicitudApoyo.CambiarEstado`) y rechaza saltos inválidos con `TransicionEstadoInvalidaException`:

```
Pendiente  ->  En revisión  ->  Aprobada
                            ->  Rechazada
```

## Modelo de datos

- **Usuario**: `Id, NombreCompleto, Email, PasswordHash, Rol, FechaRegistro`.
- **Estudiante**: `Id, UsuarioId (FK), NumeroDocumento, TipoDocumento, ProgramaAcademico, Semestre`.
- **SolicitudApoyo**: `Id, EstudianteId (FK), TipoApoyo, MontoSolicitado, Descripcion, Estado, FechaSolicitud, FechaActualizacion, AsesorId (FK)`.
- **HistorialEstado**: `Id, SolicitudId (FK), EstadoAnterior, EstadoNuevo, FechaCambio, UsuarioId (FK), Observacion`.

La autenticación usa las tablas de ASP.NET Core Identity (`AspNetUsers`, `AspNetRoles`, etc.).

## Ejecución local

### Opción A: Docker Compose (recomendada)

Levanta API, frontend y PostgreSQL local (profile `local`):

```bash
docker compose --profile local up --build
```

- Frontend: http://localhost:8080
- API + Swagger: http://localhost:5000/swagger
- Las migraciones y el seed se aplican automáticamente al arrancar el API.

### Opción B: Ejecución manual

Requisitos: .NET 8 SDK, Node 22, PostgreSQL en local.

```bash
# 1. Crear una instancia PostgreSQL y ajustar la cadena de conexión
#    en src/EduApoyos.Api/appsettings.Development.json (o variable de entorno).

# 2. API (aplica migraciones y seed al arrancar)
dotnet run --project src/EduApoyos.Api

# 3. Frontend (otra terminal)
cd frontend
npm install
npm run dev
```

El frontend en desarrollo hace proxy de `/api` hacia el API (`frontend/vite.config.ts`).

### Usuarios de prueba (seed)

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Asesor | `asesor@educapoyos.local` | `Asesor123*` |
| Estudiante | `estudiante@educapoyos.local` | `Estudiante123*` |

## Variables de entorno

Ver `.env.example`. Las principales:

| Variable | Descripción |
|----------|-------------|
| `CONNECTION_STRING` / `ConnectionStrings__Default` | Cadena de conexión a PostgreSQL |
| `JWT_KEY` / `Jwt__Key` | Clave de firma JWT (mínimo 32 caracteres) |
| `JWT_EXPIRE_MINUTES` / `Jwt__ExpireMinutes` | Expiración del token |
| `FRONTEND_ORIGIN` | Origen permitido para CORS |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución |

Los secretos no van en el código: en desarrollo se usan `appsettings.Development.json` y en producción se inyectan por variables de entorno.

## Endpoints del API

| Método | Ruta | Rol |
|--------|------|-----|
| POST | `/api/auth/register` | Público |
| POST | `/api/auth/login` | Público |
| GET | `/api/estudiantes` | Asesor |
| POST | `/api/estudiantes` | Asesor |
| GET | `/api/solicitudes` (filtros: estado, tipo, desde, hasta; paginado) | Asesor |
| POST | `/api/solicitudes` | Asesor / Estudiante |
| GET | `/api/solicitudes/{id}` (detalle + historial) | Asesor / Propietario |
| PATCH | `/api/solicitudes/{id}/estado` | Asesor |
| GET | `/api/estudiantes/{id}/solicitudes` | Estudiante (propio) |
| GET | `/api/estudiantes/me/solicitudes` | Estudiante (propio) |
| GET | `/api/estudiantes/{id}/solicitudes/{solicitudId}/constancia` | Estudiante (propio) |

Calidad del API: errores con `ProblemDetails` (RFC 7807), validaciones con FluentValidation, paginación en listados y Swagger funcional.

## Seguridad

- JWT con expiración configurable; firma HMAC-SHA256 con clave por variable de entorno.
- Contraseñas hasheadas por ASP.NET Core Identity (nunca en texto plano).
- Autorización por rol (`Asesor` / `Estudiante`) y **por recurso**: el estudiante solo ve o descarga constancias de sus propias solicitudes.
- Secretos fuera del código fuente.
- HTTPS en local: `dotnet dev-certs https --trust`. En producción se recomienda terminar TLS en el reverse proxy.

## Pruebas y cobertura

```bash
dotnet test EduApoyos.slnx --collect:"XPlat Code Coverage"
```

La capa Application supera el 70% exigido. Se cubren handlers de:

- **Auth**: registro exitoso, errores de Identity, login y credenciales inválidas.
- **Estudiantes**: creación, usuario inexistente, listado paginado.
- **Solicitudes**: creación, cambio de estado (válido e inválido), detalle con autorización, listados y filtros.
- **Constancias**: generación de archivo de texto con validación de propiedad del recurso.

## SQL requerido

En la carpeta `sql/` (adaptado a PostgreSQL):

1. `01_solicitudes_pendientes_sin_actualizacion.sql`: pendientes con más de 5 días sin actualización, por antigüedad.
2. `02_conteo_por_estado_y_tipo_ultimo_mes.sql`: conteo por estado y tipo en el último mes.
3. `03_indice_no_agrupado_solicitudes.sql`: índice compuesto justificado sobre `SolicitudesApoyo` (también creado por EF Core en la migración inicial).

## Documentación del código

Todo el código fuente está documentado en español:

| Capa | Formato | Qué incluye |
|------|---------|-------------|
| **Domain** | XML (`///`) | Entidades, enums, value objects, excepciones; reglas de validación en `<remarks>` |
| **Application** | XML | Puertos, handlers, DTOs; cada `RuleFor` de FluentValidation documentado |
| **Infrastructure / API** | XML | Repositorios, Identity, JWT, controladores (roles y códigos HTTP) |
| **Frontend** | JSDoc | Componentes, tipos, validaciones de formulario y llamadas API |
| **Tests** | XML | Cada `[Fact]` describe el escenario que valida |

Al compilar, los proyectos .NET generan archivos `.xml` de documentación (`GenerateDocumentationFile`). En el IDE, pasa el cursor sobre tipos y métodos para ver la documentación inline.

Ejemplo de puerto documentado: `src/EduApoyos.Application/Ports/ISolicitudRepository.cs` (cada método con `summary`, `param`, `returns` y `remarks` de comportamiento).

## CI/CD

GitHub Actions en `.github/workflows/`:

- `ci-backend.yml`: `restore` → `build` (Release) → `test` (con cobertura) → `publish` (artefacto del API).
- `ci-frontend.yml`: `npm ci` → `lint` → `build`.

## Decisiones relevantes y pendientes

Prioricé una arquitectura verificable: dominio rico con agregados y value objects, casos de uso aislados en handlers por feature, puertos que desacoplan Identity/JWT de la API, y controladores delgados. Elegí PostgreSQL por simplicidad operativa y mantuve el código portable mediante EF Core. La constancia de RF-04 se entrega en texto plano descargable para mantener el alcance acotado con calidad.

Con más tiempo dejaría: generación de constancia en PDF real (por ejemplo QuestPDF), pruebas de integración con `WebApplicationFactory`, refresh tokens y revocación de JWT, filtros más ricos en el frontend (fechas y export), y un sistema de componentes con estados de carga y notificaciones centralizadas.
