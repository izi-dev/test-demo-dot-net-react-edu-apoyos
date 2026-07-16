/**
 * Tabla reutilizable para listar solicitudes de apoyo económico.
 */

import { Link } from 'react-router-dom'
import { useState } from 'react'
import { descargarConstancia } from '../api/constancia'
import { StatusBadge } from './StatusBadge'
import { ETIQUETA_TIPO, formatearFecha, formatearMonto } from '../lib/labels'
import type { Solicitud } from '../types'

type SolicitudTableProps = {
  solicitudes: Solicitud[]
  loading: boolean
  /** Muestra el botón de descarga de constancia (solo para estudiantes). */
  mostrarDescargaConstancia?: boolean
  emptyTitle?: string
  emptyDescription?: string
  emptyActionHref?: string
  emptyActionLabel?: string
}

/**
 * Renderiza un listado tabular de solicitudes con enlace al detalle.
 */
export function SolicitudTable({
  solicitudes,
  loading,
  mostrarDescargaConstancia = false,
  emptyTitle = 'Sin solicitudes',
  emptyDescription = 'Cuando existan registros, aparecerán aquí.',
  emptyActionHref,
  emptyActionLabel,
}: SolicitudTableProps) {
  const [descargandoId, setDescargandoId] = useState<string | null>(null)
  const [errorDescarga, setErrorDescarga] = useState('')

  async function handleDescargarConstancia(solicitud: Solicitud) {
    setDescargandoId(solicitud.id)
    setErrorDescarga('')
    try {
      await descargarConstancia(solicitud.estudianteId, solicitud.id)
    } catch {
      setErrorDescarga('No se pudo descargar la constancia. Intenta de nuevo.')
    } finally {
      setDescargandoId(null)
    }
  }

  if (loading) {
    return (
      <div className="loading-block" aria-busy="true" aria-live="polite">
        <div className="skeleton-lines">
          <div className="skeleton-line" />
          <div className="skeleton-line w80" />
          <div className="skeleton-line w60" />
        </div>
      </div>
    )
  }

  if (solicitudes.length === 0) {
    return (
      <div className="empty">
        <h3>{emptyTitle}</h3>
        <p>{emptyDescription}</p>
        {emptyActionHref && emptyActionLabel && (
          <Link className="btn btn-primary" to={emptyActionHref}>
            {emptyActionLabel}
          </Link>
        )}
      </div>
    )
  }

  return (
    <>
      {errorDescarga && <p className="error">{errorDescarga}</p>}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Estudiante</th>
              <th>Tipo</th>
              <th>Monto</th>
              <th>Estado</th>
              <th>Fecha</th>
              <th>
                <span className="visually-hidden">Acciones</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {solicitudes.map((solicitud) => (
              <tr key={solicitud.id}>
                <td>
                  <strong>{solicitud.nombreEstudiante}</strong>
                </td>
                <td>{ETIQUETA_TIPO[solicitud.tipoApoyo]}</td>
                <td>{formatearMonto(solicitud.montoSolicitado)}</td>
                <td>
                  <StatusBadge estado={solicitud.estado} />
                </td>
                <td className="cell-muted">{formatearFecha(solicitud.fechaSolicitud)}</td>
                <td className="table-actions">
                  <Link to={`/solicitudes/${solicitud.id}`}>Ver detalle</Link>
                  {mostrarDescargaConstancia && (
                    <button
                      type="button"
                      className="btn-secondary"
                      disabled={descargandoId === solicitud.id}
                      onClick={() => handleDescargarConstancia(solicitud)}
                    >
                      {descargandoId === solicitud.id ? 'Descargando...' : 'Constancia'}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
