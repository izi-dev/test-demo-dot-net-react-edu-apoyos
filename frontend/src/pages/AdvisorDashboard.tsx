/**
 * Panel del asesor: listado filtrable de todas las solicitudes.
 */

import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { SolicitudTable } from '../components/SolicitudTable'
import type { PagedResult, Solicitud } from '../types'

/**
 * Dashboard principal para asesores con filtros por estado y tipo de apoyo.
 *
 * Estado interno:
 * - `estado` — filtro por estado de solicitud; cadena vacía significa «todos».
 * - `tipo` — filtro por tipo de apoyo; cadena vacía significa «todos».
 * - `data` — resultado paginado devuelto por el API.
 * - `loading` — indica recarga de datos al cambiar filtros.
 * - `error` — mensaje si falla la petición al API.
 *
 * Llamadas API:
 * - `GET /api/solicitudes` — lista solicitudes con parámetros opcionales `estado` y `tipo`.
 */
export function AdvisorDashboard() {
  const [estado, setEstado] = useState('')
  const [tipo, setTipo] = useState('')
  const [data, setData] = useState<PagedResult<Solicitud> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')
    api
      .get<PagedResult<Solicitud>>('/api/solicitudes', {
        params: { estado: estado || undefined, tipo: tipo || undefined },
      })
      .then((response) => setData(response.data))
      .catch(() => setError('No fue posible cargar las solicitudes. Intenta de nuevo.'))
      .finally(() => setLoading(false))
  }, [estado, tipo])

  const total = data?.totalItems ?? 0

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Panel asesor</p>
          <h1>Solicitudes</h1>
          <p className="support">
            Revisa, filtra y da seguimiento a las solicitudes de apoyo económico.
          </p>
        </div>
        <Link className="btn btn-primary" to="/solicitudes/nueva">
          Nueva solicitud
        </Link>
      </header>

      <div className="filters">
        <label>
          Estado
          <select value={estado} onChange={(event) => setEstado(event.target.value)}>
            <option value="">Todos</option>
            <option value="Pendiente">Pendiente</option>
            <option value="EnRevision">En revisión</option>
            <option value="Aprobada">Aprobada</option>
            <option value="Rechazada">Rechazada</option>
          </select>
        </label>
        <label>
          Tipo de apoyo
          <select value={tipo} onChange={(event) => setTipo(event.target.value)}>
            <option value="">Todos</option>
            <option value="Beca">Beca</option>
            <option value="Credito">Crédito</option>
            <option value="Subsidio">Subsidio</option>
          </select>
        </label>
      </div>

      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <div className="list-meta">
          <span>
            {total === 1 ? '1 solicitud' : `${total} solicitudes`}
            {(estado || tipo) && ' con los filtros actuales'}
          </span>
        </div>
      )}

      <SolicitudTable
        solicitudes={data?.items ?? []}
        loading={loading}
        emptyTitle="No hay solicitudes"
        emptyDescription="Ajusta los filtros o crea la primera solicitud de apoyo."
        emptyActionHref="/solicitudes/nueva"
        emptyActionLabel="Crear solicitud"
      />
    </section>
  )
}
