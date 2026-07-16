/**
 * Tabla reutilizable para listar solicitudes de apoyo económico.
 */

import { Link } from 'react-router-dom'
import { useState } from 'react'
import { descargarConstancia } from '../api/constancia'
import type { Solicitud } from '../types'

type SolicitudTableProps = {
  solicitudes: Solicitud[]
  loading: boolean
  /** Muestra el botón de descarga de constancia (solo para estudiantes). */
  mostrarDescargaConstancia?: boolean
}

/**
 * Renderiza un listado tabular de solicitudes con enlace al detalle.
 */
export function SolicitudTable({
  solicitudes,
  loading,
  mostrarDescargaConstancia = false,
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
    return <p>Cargando solicitudes...</p>
  }

  if (solicitudes.length === 0) {
    return <p className="empty">No hay solicitudes para mostrar.</p>
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
              <th></th>
            </tr>
          </thead>
          <tbody>
            {solicitudes.map((solicitud) => (
              <tr key={solicitud.id}>
                <td>{solicitud.nombreEstudiante}</td>
                <td>{solicitud.tipoApoyo}</td>
                <td>{solicitud.montoSolicitado.toLocaleString('es-CO')}</td>
                <td>
                  <span className={`status ${solicitud.estado}`}>{solicitud.estado}</span>
                </td>
                <td>{new Date(solicitud.fechaSolicitud).toLocaleDateString('es-CO')}</td>
                <td className="table-actions">
                  <Link to={`/solicitudes/${solicitud.id}`}>Ver</Link>
                  {mostrarDescargaConstancia && (
                    <button
                      type="button"
                      className="secondary-button"
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
