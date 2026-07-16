/**
 * Vista de detalle de una solicitud con historial y acciones según el rol.
 */

import { isAxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { api } from '../api/client'
import { descargarConstancia } from '../api/constancia'
import { useAuth } from '../auth/AuthContext'
import type { EstadoSolicitud, Solicitud } from '../types'

const TRANSICIONES: Record<EstadoSolicitud, EstadoSolicitud[]> = {
  Pendiente: ['EnRevision'],
  EnRevision: ['Aprobada', 'Rechazada'],
  Aprobada: [],
  Rechazada: [],
}

const ETIQUETAS: Record<EstadoSolicitud, string> = {
  Pendiente: 'Pendiente',
  EnRevision: 'En revisión',
  Aprobada: 'Aprobada',
  Rechazada: 'Rechazada',
}

/**
 * Muestra la información completa de una solicitud.
 * Los asesores pueden cambiar el estado; los estudiantes pueden descargar la constancia.
 */
export function SolicitudDetail() {
  const { auth } = useAuth()
  const { id } = useParams()
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null)
  const [estado, setEstado] = useState<EstadoSolicitud | ''>('')
  const [observacion, setObservacion] = useState('')
  const [descargando, setDescargando] = useState(false)
  const [actualizando, setActualizando] = useState(false)
  const [error, setError] = useState('')

  const siguientes = useMemo(
    () => (solicitud ? TRANSICIONES[solicitud.estado] : []),
    [solicitud],
  )

  useEffect(() => {
    if (!id) {
      return
    }

    api.get<Solicitud>(`/api/solicitudes/${id}`).then((response) => {
      setSolicitud(response.data)
      const opciones = TRANSICIONES[response.data.estado]
      setEstado(opciones[0] ?? '')
    })
  }, [id])

  async function cambiarEstado() {
    if (!id || !estado) {
      return
    }

    setActualizando(true)
    setError('')
    try {
      const { data } = await api.patch<Solicitud>(`/api/solicitudes/${id}/estado`, {
        estado,
        observacion: observacion || null,
      })
      setSolicitud(data)
      setObservacion('')
      const opciones = TRANSICIONES[data.estado]
      setEstado(opciones[0] ?? '')
    } catch (err) {
      if (isAxiosError(err)) {
        const detail = err.response?.data?.detail as string | undefined
        setError(detail ?? 'No se pudo actualizar el estado.')
      } else {
        setError('No se pudo actualizar el estado.')
      }
    } finally {
      setActualizando(false)
    }
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
          <span className={`status ${solicitud.estado}`}>{ETIQUETAS[solicitud.estado]}</span>
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
            {siguientes.length === 0 ? (
              <p>Esta solicitud ya está cerrada y no admite más cambios.</p>
            ) : (
              <>
                <select
                  value={estado}
                  onChange={(event) => setEstado(event.target.value as EstadoSolicitud)}
                >
                  {siguientes.map((opcion) => (
                    <option key={opcion} value={opcion}>
                      {ETIQUETAS[opcion]}
                    </option>
                  ))}
                </select>
                <textarea
                  placeholder="Observación"
                  value={observacion}
                  onChange={(event) => setObservacion(event.target.value)}
                />
                <button type="button" disabled={actualizando || !estado} onClick={cambiarEstado}>
                  {actualizando ? 'Actualizando...' : 'Actualizar'}
                </button>
              </>
            )}
          </aside>
        )}
      </div>
      <section className="panel">
        <h2>Historial</h2>
        {solicitud.historial.map((item) => (
          <p key={item.id}>
            {new Date(item.fechaCambio).toLocaleString('es-CO')} —{' '}
            {item.estadoAnterior ? ETIQUETAS[item.estadoAnterior] : 'Creación'} →{' '}
            {ETIQUETAS[item.estadoNuevo]}: {item.observacion}
          </p>
        ))}
      </section>
    </section>
  )
}
