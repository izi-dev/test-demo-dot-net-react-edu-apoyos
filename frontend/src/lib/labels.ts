/**
 * Etiquetas y utilidades de presentación compartidas en la UI.
 */

import type { EstadoSolicitud, TipoApoyo } from '../types'

export const ETIQUETA_ESTADO: Record<EstadoSolicitud, string> = {
  Pendiente: 'Pendiente',
  EnRevision: 'En revisión',
  Aprobada: 'Aprobada',
  Rechazada: 'Rechazada',
}

export const ETIQUETA_TIPO: Record<TipoApoyo, string> = {
  Beca: 'Beca',
  Credito: 'Crédito',
  Subsidio: 'Subsidio',
}

export const TRANSICIONES: Record<EstadoSolicitud, EstadoSolicitud[]> = {
  Pendiente: ['EnRevision'],
  EnRevision: ['Aprobada', 'Rechazada'],
  Aprobada: [],
  Rechazada: [],
}

export function formatearMonto(valor: number): string {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    maximumFractionDigits: 0,
  }).format(valor)
}

export function formatearFecha(valor: string): string {
  return new Date(valor).toLocaleDateString('es-CO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export function formatearFechaHora(valor: string): string {
  return new Date(valor).toLocaleString('es-CO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
