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

/** Respuesta del endpoint de autenticación. */
export type AuthResponse = {
  token: string
  expiresAt: string
  userId: string
  nombreCompleto: string
  email: string
  rol: RolUsuario
}

/** Resultado paginado genérico devuelto por listados de la API. */
export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

/** Datos de un estudiante registrado en el sistema. */
export type Estudiante = {
  id: string
  usuarioId: string
  nombreCompleto: string
  email: string
  numeroDocumento: string
  tipoDocumento: string
  programaAcademico: string
  semestre: number
}

/** Entrada del historial de cambios de estado de una solicitud. */
export type HistorialEstado = {
  id: string
  estadoAnterior?: EstadoSolicitud
  estadoNuevo: EstadoSolicitud
  fechaCambio: string
  observacion: string
}

/** Solicitud de apoyo económico con su historial de estados. */
export type Solicitud = {
  id: string
  estudianteId: string
  nombreEstudiante: string
  tipoApoyo: TipoApoyo
  montoSolicitado: number
  descripcion: string
  estado: EstadoSolicitud
  fechaSolicitud: string
  fechaActualizacion: string
  historial: HistorialEstado[]
}
