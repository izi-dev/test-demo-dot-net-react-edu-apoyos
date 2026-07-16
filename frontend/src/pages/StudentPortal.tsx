/**
 * Portal de autogestión del estudiante: consulta de solicitudes propias.
 */

import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { SolicitudTable } from '../components/SolicitudTable'
import type { Solicitud } from '../types'

/**
 * Lista las solicitudes del estudiante autenticado mediante el endpoint `/me`.
 *
 * Estado interno:
 * - `solicitudes` — arreglo de solicitudes propias del estudiante.
 * - `loading` — indica carga inicial de datos.
 * - `error` — mensaje si falla la petición al API.
 *
 * Llamadas API:
 * - `GET /api/estudiantes/me/solicitudes` — obtiene todas las solicitudes del usuario en sesión.
 */
export function StudentPortal() {
  const [solicitudes, setSolicitudes] = useState<Solicitud[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')

    api
      .get<Solicitud[]>('/api/estudiantes/me/solicitudes')
      .then((response) => setSolicitudes(response.data))
      .catch(() => setError('No fue posible cargar tus solicitudes.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Portal estudiante</p>
          <h1>Mis solicitudes</h1>
          <p className="support">
            Consulta el estado de tus apoyos y descarga la constancia cuando lo necesites.
          </p>
        </div>
        <Link className="btn btn-primary" to="/solicitudes/nueva">
          Nueva solicitud
        </Link>
      </header>

      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <div className="list-meta">
          <span>
            {solicitudes.length === 1
              ? '1 solicitud registrada'
              : `${solicitudes.length} solicitudes registradas`}
          </span>
        </div>
      )}

      <SolicitudTable
        solicitudes={solicitudes}
        loading={loading}
        mostrarDescargaConstancia
        emptyTitle="Aún no tienes solicitudes"
        emptyDescription="Crea tu primera solicitud de beca, crédito o subsidio."
        emptyActionHref="/solicitudes/nueva"
        emptyActionLabel="Crear solicitud"
      />
    </section>
  )
}
