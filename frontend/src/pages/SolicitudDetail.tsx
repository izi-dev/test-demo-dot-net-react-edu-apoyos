/**
 * Vista de detalle de una solicitud con historial y acciones según el rol.
 */

import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { api } from '../api/client'
import { descargarConstancia } from '../api/constancia'
import { useAuth } from '../auth/AuthContext'
import type { EstadoSolicitud, Solicitud } from '../types'

/**
 * Muestra la información completa de una solicitud.
 * Los asesores pueden cambiar el estado; los estudiantes pueden descargar la constancia.
 */
export function SolicitudDetail() {
  const { auth } = useAuth()
  const { id } = useParams()
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null)
  const [estado, setEstado] = useState<EstadoSolicitud>('EnRevision')
  const [observacion, setObservacion] = useState('')
  const [descargando, setDescargando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) {
      return
    }

    api.get<Solicitud>(`/api/solicitudes/${id}`).then((response) => setSolicitud(response.data))
  }, [id])

  async function cambiarEstado() {
    if (!id) {
      return
    }

    const { data } = await api.patch<Solicitud>(`/api/solicitudes/${id}/estado`, {
      estado,
      observacion,
    })
    setSolicitud(data)
    setObservacion('')
  }

  async function handleDescargarConstancia() {
    if (!solicitud) {
      return
    }

    setDescargando(true)
    setError('')
    try {
      await descargarConstancia(solicitud.estudianteId, solicitud.id)
    } catch {
      setError('No se pudo descargar la constancia. Intenta de nuevo.')
    } finally {
      setDescargando(false)
    }
  }

  if (!solicitud) {
    return <section className="page">Cargando...</section>
  }

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Detalle</p>
          <h1>{solicitud.nombreEstudiante}</h1>
        </div>
        <div className="header-actions">
          <span className={`status ${solicitud.estado}`}>{solicitud.estado}</span>
          {auth?.rol === 'Estudiante' && (
            <button
              type="button"
              className="secondary-button"
              disabled={descargando}
              onClick={handleDescargarConstancia}
            >
              {descargando ? 'Descargando...' : 'Descargar constancia'}
            </button>
          )}
        </div>
      </header>
      {error && <p className="error">{error}</p>}
      <div className="detail-grid">
        <article className="panel">
          <p>
            <strong>Tipo:</strong> {solicitud.tipoApoyo}
          </p>
          <p>
            <strong>Monto:</strong> {solicitud.montoSolicitado.toLocaleString('es-CO')}
          </p>
          <p>
            <strong>Descripción:</strong> {solicitud.descripcion}
          </p>
        </article>
        {auth?.rol === 'Asesor' && (
          <aside className="panel form-panel">
            <h2>Cambiar estado</h2>
            <select
              value={estado}
              onChange={(event) => setEstado(event.target.value as EstadoSolicitud)}
            >
              <option value="EnRevision">En revisión</option>
              <option value="Aprobada">Aprobada</option>
              <option value="Rechazada">Rechazada</option>
            </select>
            <textarea
              placeholder="Observación"
              value={observacion}
              onChange={(event) => setObservacion(event.target.value)}
            />
            <button type="button" onClick={cambiarEstado}>
              Actualizar
            </button>
          </aside>
        )}
      </div>
      <section className="panel">
        <h2>Historial</h2>
        {solicitud.historial.map((item) => (
          <p key={item.id}>
            {new Date(item.fechaCambio).toLocaleString('es-CO')} —{' '}
            {item.estadoAnterior ?? 'Creación'} → {item.estadoNuevo}: {item.observacion}
          </p>
        ))}
      </section>
    </section>
  )
}
