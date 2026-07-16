/**
 * Indicador visual del estado de una solicitud.
 */

import { ETIQUETA_ESTADO } from '../lib/labels'
import type { EstadoSolicitud } from '../types'

/**
 * Props del componente {@link StatusBadge}.
 */
type StatusBadgeProps = {
  /** Estado actual de la solicitud que determina el estilo y la etiqueta mostrada. */
  estado: EstadoSolicitud
}

/**
 * Renderiza una etiqueta coloreada con el nombre legible del estado de la solicitud.
 *
 * @param props - Propiedades del componente.
 * @param props.estado - Estado de la solicitud (`Pendiente`, `EnRevision`, `Aprobada` o `Rechazada`).
 */
export function StatusBadge({ estado }: StatusBadgeProps) {
  return <span className={`status status-${estado}`}>{ETIQUETA_ESTADO[estado]}</span>
}
