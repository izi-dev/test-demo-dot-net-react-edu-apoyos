/**
 * Etiquetas y utilidades de presentación compartidas en la UI.
 */

import type { EstadoSolicitud, TipoApoyo } from '../types'

/**
 * Mapa de estados de solicitud a etiquetas legibles en español para la interfaz.
 */
export const ETIQUETA_ESTADO: Record<EstadoSolicitud, string> = {
  Pendiente: 'Pendiente',
  EnRevision: 'En revisión',
  Aprobada: 'Aprobada',
  Rechazada: 'Rechazada',
}

/**
 * Mapa de tipos de apoyo económico a etiquetas legibles en español para la interfaz.
 */
export const ETIQUETA_TIPO: Record<TipoApoyo, string> = {
  Beca: 'Beca',
  Credito: 'Crédito',
  Subsidio: 'Subsidio',
}

/**
 * Transiciones de estado permitidas según el flujo institucional.
 * Cada clave indica los estados destino válidos desde el estado actual.
 */
export const TRANSICIONES: Record<EstadoSolicitud, EstadoSolicitud[]> = {
  Pendiente: ['EnRevision'],
  EnRevision: ['Aprobada', 'Rechazada'],
  Aprobada: [],
  Rechazada: [],
}

/**
 * Formatea un valor numérico como moneda colombiana (COP) sin decimales.
 *
 * @param valor - Monto en pesos colombianos.
 * @returns Cadena formateada, por ejemplo `$ 2.500.000`.
 */
export function formatearMonto(valor: number): string {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    maximumFractionDigits: 0,
  }).format(valor)
}

/**
 * Formatea una fecha ISO en formato corto local (`es-CO`).
 *
 * @param valor - Fecha en cadena ISO 8601.
 * @returns Fecha legible, por ejemplo `16 jul 2026`.
 */
export function formatearFecha(valor: string): string {
  return new Date(valor).toLocaleDateString('es-CO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

/**
 * Formatea una fecha y hora ISO en formato local (`es-CO`).
 *
 * @param valor - Fecha y hora en cadena ISO 8601.
 * @returns Fecha y hora legibles, por ejemplo `16 jul 2026, 04:30 p. m.`.
 */
export function formatearFechaHora(valor: string): string {
  return new Date(valor).toLocaleString('es-CO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
