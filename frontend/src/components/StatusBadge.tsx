/**
 * Indicador visual del estado de una solicitud.
 */

import { ETIQUETA_ESTADO } from '../lib/labels'
import type { EstadoSolicitud } from '../types'

type StatusBadgeProps = {
  estado: EstadoSolicitud
}

export function StatusBadge({ estado }: StatusBadgeProps) {
  return <span className={`status status-${estado}`}>{ETIQUETA_ESTADO[estado]}</span>
}
