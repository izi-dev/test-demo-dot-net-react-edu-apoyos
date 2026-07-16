/**
 * Tipos compartidos del frontend de EduApoyos.
 * Reflejan los contratos expuestos por la API .NET.
 */

/** Roles de usuario reconocidos por la aplicación. */
export type RolUsuario = 'Asesor' | 'Estudiante'

/** Estados posibles de una solicitud de apoyo económico. */
export type EstadoSolicitud = 'Pendiente' | 'EnRevision' | 'Aprobada' | 'Rechazada'

/** Tipos de apoyo económico disponibles. */
export type TipoApoyo = 'Beca' | 'Credito' | 'Subsidio'

/**
 * Respuesta del endpoint `POST /api/auth/login`.
 * Contiene el JWT y los datos del usuario autenticado.
 */
export type AuthResponse = {
  /** Token JWT para autorizar peticiones subsiguientes. */
  token: string
  /** Fecha y hora de expiración del token en formato ISO 8601. */
  expiresAt: string
  /** Identificador único del usuario en el sistema. */
  userId: string
  /** Nombre completo del usuario para mostrar en la UI. */
  nombreCompleto: string
  /** Correo electrónico institucional del usuario. */
  email: string
  /** Rol que determina las rutas y acciones disponibles. */
  rol: RolUsuario
}

/**
 * Resultado paginado genérico devuelto por listados de la API.
 *
 * @typeParam T - Tipo de cada elemento en `items`.
 */
export type PagedResult<T> = {
  /** Elementos de la página actual. */
  items: T[]
  /** Número de página solicitada (base 1). */
  page: number
  /** Cantidad máxima de elementos por página. */
  pageSize: number
  /** Total de registros que cumplen el filtro. */
  totalItems: number
  /** Total de páginas calculadas a partir de `totalItems` y `pageSize`. */
  totalPages: number
}

/** Datos de un estudiante registrado en el sistema. */
export type Estudiante = {
  /** Identificador único del estudiante. */
  id: string
  /** Identificador del usuario de autenticación asociado. */
  usuarioId: string
  /** Nombre completo del estudiante. */
  nombreCompleto: string
  /** Correo electrónico del estudiante. */
  email: string
  /** Número de documento de identidad. */
  numeroDocumento: string
  /** Tipo de documento (por ejemplo, cédula o pasaporte). */
  tipoDocumento: string
  /** Programa académico en el que está matriculado. */
  programaAcademico: string
  /** Semestre académico actual. */
  semestre: number
}

/** Entrada del historial de cambios de estado de una solicitud. */
export type HistorialEstado = {
  /** Identificador único del registro de historial. */
  id: string
  /** Estado anterior; ausente en el registro de creación. */
  estadoAnterior?: EstadoSolicitud
  /** Estado resultante tras el cambio. */
  estadoNuevo: EstadoSolicitud
  /** Fecha y hora del cambio en formato ISO 8601. */
  fechaCambio: string
  /** Comentario u observación opcional del asesor. */
  observacion: string
}

/** Solicitud de apoyo económico con su historial de estados. */
export type Solicitud = {
  /** Identificador único de la solicitud. */
  id: string
  /** Identificador del estudiante solicitante. */
  estudianteId: string
  /** Nombre del estudiante para mostrar en listados y detalle. */
  nombreEstudiante: string
  /** Tipo de apoyo solicitado (beca, crédito o subsidio). */
  tipoApoyo: TipoApoyo
  /** Monto solicitado en pesos colombianos (COP). */
  montoSolicitado: number
  /** Descripción o justificación de la solicitud. */
  descripcion: string
  /** Estado actual en el flujo de revisión. */
  estado: EstadoSolicitud
  /** Fecha de creación de la solicitud en formato ISO 8601. */
  fechaSolicitud: string
  /** Fecha de la última modificación en formato ISO 8601. */
  fechaActualizacion: string
  /** Lista cronológica de cambios de estado. */
  historial: HistorialEstado[]
}
