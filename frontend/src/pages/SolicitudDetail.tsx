/**
 * Vista de detalle de una solicitud con historial y acciones según el rol.
 */

import { isAxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { descargarConstancia } from '../api/constancia'
import { StatusBadge } from '../components/StatusBadge'
import { useAuth } from '../auth/AuthContext'
import {
  ETIQUETA_ESTADO,
  ETIQUETA_TIPO,
  TRANSICIONES,
  formatearFechaHora,
  formatearMonto,
} from '../lib/labels'
import type { EstadoSolicitud, Solicitud } from '../types'

/**
 * Pasos del flujo visual mostrados en la barra de progreso (excluye `Rechazada`, que se muestra aparte).
 */
const FLUJO: EstadoSolicitud[] = ['Pendiente', 'EnRevision', 'Aprobada']

/**
 * Muestra la información completa de una solicitud.
 * Los asesores pueden cambiar el estado; los estudiantes pueden descargar la constancia.
 *
 * Parámetros de ruta:
 * - `id` — identificador de la solicitud (`useParams`).
 *
 * Estado interno:
 * - `solicitud` — datos completos con historial.
 * - `cargando` — indica carga inicial.
 * - `estado` — nuevo estado seleccionado por el asesor.
 * - `observacion` — comentario opcional al cambiar estado.
 * - `descargando` — descarga de constancia en curso.
 * - `actualizando` — cambio de estado en curso.
 * - `error` — mensaje de error de carga, actualización o descarga.
 * - `exito` — confirmación tras actualizar el estado.
 *
 * Llamadas API:
 * - `GET /api/solicitudes/{id}` — carga el detalle al montar o al cambiar `id`.
 * - `PATCH /api/solicitudes/{id}/estado` — actualiza estado (solo Asesor); body `{ estado, observacion }`.
 * - `descargarConstancia` — descarga constancia (solo Estudiante).
 */
export function SolicitudDetail() {
  const { auth } = useAuth()
  const { id } = useParams()
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null)
  const [cargando, setCargando] = useState(true)
  const [estado, setEstado] = useState<EstadoSolicitud | ''>('')
  const [observacion, setObservacion] = useState('')
  const [descargando, setDescargando] = useState(false)
  const [actualizando, setActualizando] = useState(false)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')

  const siguientes = useMemo(
    () => (solicitud ? TRANSICIONES[solicitud.estado] : []),
    [solicitud],
  )

  useEffect(() => {
    if (!id) {
      return
    }

    setCargando(true)
    api
      .get<Solicitud>(`/api/solicitudes/${id}`)
      .then((response) => {
        setSolicitud(response.data)
        const opciones = TRANSICIONES[response.data.estado]
        setEstado(opciones[0] ?? '')
      })
      .catch(() => setError('No se pudo cargar la solicitud.'))
      .finally(() => setCargando(false))
  }, [id])

  async function cambiarEstado() {
    if (!id || !estado) {
      return
    }

    setActualizando(true)
    setError('')
    setExito('')
    try {
      const { data } = await api.patch<Solicitud>(`/api/solicitudes/${id}/estado`, {
        estado,
        observacion: observacion || null,
      })
      setSolicitud(data)
      setObservacion('')
      const opciones = TRANSICIONES[data.estado]
      setEstado(opciones[0] ?? '')
      setExito(`Estado actualizado a «${ETIQUETA_ESTADO[data.estado]}».`)
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

  const volverA = auth?.rol === 'Asesor' ? '/asesor' : '/portal'

  if (cargando) {
    return (
      <section className="page">
        <div className="loading-block" aria-busy="true">
          <div className="skeleton-lines">
            <div className="skeleton-line w60" />
            <div className="skeleton-line" />
            <div className="skeleton-line w80" />
          </div>
        </div>
      </section>
    )
  }

  if (!solicitud) {
    return (
      <section className="page">
        <Link className="back-link" to={volverA}>
          ← Volver
        </Link>
        <div className="empty">
          <h3>Solicitud no disponible</h3>
          <p>{error || 'No encontramos esta solicitud.'}</p>
          <Link className="btn btn-primary" to={volverA}>
            Ir al listado
          </Link>
        </div>
      </section>
    )
  }

  function pasoClase(paso: EstadoSolicitud): string {
    if (!solicitud) {
      return 'flow-step'
    }

    if (solicitud.estado === 'Rechazada') {
      if (paso === 'Pendiente') {
        return 'flow-step done'
      }
      if (paso === 'EnRevision') {
        return 'flow-step done'
      }
      return 'flow-step'
    }

    const orden: EstadoSolicitud[] = ['Pendiente', 'EnRevision', 'Aprobada']
    const actual = orden.indexOf(solicitud.estado)
    const idx = orden.indexOf(paso)
    if (idx < actual) {
      return 'flow-step done'
    }
    if (idx === actual) {
      return 'flow-step current'
    }
    return 'flow-step'
  }

  return (
    <section className="page">
      <Link className="back-link" to={volverA}>
        ← Volver al listado
      </Link>

      <header className="page-header">
        <div>
          <p className="eyebrow">Detalle de solicitud</p>
          <h1>{solicitud.nombreEstudiante}</h1>
          <p className="support">{ETIQUETA_TIPO[solicitud.tipoApoyo]} · seguimiento del caso</p>
        </div>
        <div className="header-actions">
          <StatusBadge estado={solicitud.estado} />
          {auth?.rol === 'Estudiante' && (
            <button
              type="button"
              className="btn-secondary"
              disabled={descargando}
              onClick={handleDescargarConstancia}
            >
              {descargando ? 'Descargando...' : 'Descargar constancia'}
            </button>
          )}
        </div>
      </header>

      {error && <p className="error">{error}</p>}
      {exito && <p className="alert alert-info">{exito}</p>}

      <div className="flow-steps" aria-label="Flujo de estados">
        {FLUJO.map((paso) => (
          <span key={paso} className={pasoClase(paso)}>
            {ETIQUETA_ESTADO[paso]}
          </span>
        ))}
        {solicitud.estado === 'Rechazada' && (
          <span className="flow-step current">{ETIQUETA_ESTADO.Rechazada}</span>
        )}
      </div>

      <div className="detail-grid">
        <article className="panel">
          <dl className="detail-facts">
            <div className="fact-row">
              <dt>Tipo de apoyo</dt>
              <dd>{ETIQUETA_TIPO[solicitud.tipoApoyo]}</dd>
            </div>
            <div className="fact-row">
              <dt>Monto solicitado</dt>
              <dd className="amount">{formatearMonto(solicitud.montoSolicitado)}</dd>
            </div>
            <div className="fact-row">
              <dt>Descripción</dt>
              <dd>{solicitud.descripcion}</dd>
            </div>
            <div className="fact-row">
              <dt>Última actualización</dt>
              <dd>{formatearFechaHora(solicitud.fechaActualizacion)}</dd>
            </div>
          </dl>
        </article>

        {auth?.rol === 'Asesor' && (
          <aside className="panel action-panel form-stack">
            <h2>Cambiar estado</h2>
            {siguientes.length === 0 ? (
              <p className="panel-lead">
                Esta solicitud ya está cerrada. No admite más cambios de estado.
              </p>
            ) : (
              <>
                <p className="panel-lead">
                  Siguiente paso permitido según el flujo institucional.
                </p>
                <label>
                  Nuevo estado
                  <select
                    value={estado}
                    onChange={(event) => setEstado(event.target.value as EstadoSolicitud)}
                  >
                    {siguientes.map((opcion) => (
                      <option key={opcion} value={opcion}>
                        {ETIQUETA_ESTADO[opcion]}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  Observación
                  <span className="field-hint">Opcional · visible en el historial</span>
                  <textarea
                    placeholder="Motivo o comentario del cambio"
                    value={observacion}
                    onChange={(event) => setObservacion(event.target.value)}
                  />
                </label>
                <button
                  type="button"
                  className="btn btn-primary"
                  disabled={actualizando || !estado}
                  onClick={cambiarEstado}
                >
                  {actualizando ? 'Actualizando...' : 'Guardar cambio'}
                </button>
              </>
            )}
          </aside>
        )}
      </div>

      <section className="panel">
        <h2>Historial</h2>
        {solicitud.historial.length === 0 ? (
          <p className="panel-lead">Sin movimientos registrados.</p>
        ) : (
          <ol className="timeline">
            {[...solicitud.historial]
              .sort(
                (a, b) => new Date(b.fechaCambio).getTime() - new Date(a.fechaCambio).getTime(),
              )
              .map((item) => (
                <li key={item.id}>
                  <span className="timeline-time">{formatearFechaHora(item.fechaCambio)}</span>
                  <span className="timeline-title">
                    {item.estadoAnterior
                      ? `${ETIQUETA_ESTADO[item.estadoAnterior]} → ${ETIQUETA_ESTADO[item.estadoNuevo]}`
                      : `Creación · ${ETIQUETA_ESTADO[item.estadoNuevo]}`}
                  </span>
                  {item.observacion && <p className="timeline-note">{item.observacion}</p>}
                </li>
              ))}
          </ol>
        )}
      </section>
    </section>
  )
}
