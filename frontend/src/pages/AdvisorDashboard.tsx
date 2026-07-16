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
 */
export function AdvisorDashboard() {
  const [estado, setEstado] = useState('')
  const [tipo, setTipo] = useState('')
  const [data, setData] = useState<PagedResult<Solicitud> | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    api
      .get<PagedResult<Solicitud>>('/api/solicitudes', {
        params: { estado: estado || undefined, tipo: tipo || undefined },
      })
      .then((response) => setData(response.data))
      .finally(() => setLoading(false))
  }, [estado, tipo])

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Panel asesor</p>
          <h1>Solicitudes de apoyo</h1>
        </div>
        <Link className="primary-link" to="/solicitudes/nueva">
          Crear solicitud
        </Link>
      </header>
      <div className="filters">
        <select value={estado} onChange={(event) => setEstado(event.target.value)}>
          <option value="">Todos los estados</option>
          <option value="Pendiente">Pendiente</option>
          <option value="EnRevision">En revisión</option>
          <option value="Aprobada">Aprobada</option>
          <option value="Rechazada">Rechazada</option>
        </select>
        <select value={tipo} onChange={(event) => setTipo(event.target.value)}>
          <option value="">Todos los apoyos</option>
          <option value="Beca">Beca</option>
          <option value="Credito">Crédito</option>
          <option value="Subsidio">Subsidio</option>
        </select>
      </div>
      <SolicitudTable solicitudes={data?.items ?? []} loading={loading} />
    </section>
  )
}
