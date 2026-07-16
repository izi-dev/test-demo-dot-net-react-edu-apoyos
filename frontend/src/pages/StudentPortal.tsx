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
        </div>
        <Link className="primary-link" to="/solicitudes/nueva">
          Crear solicitud
        </Link>
      </header>
      {error && <p className="error">{error}</p>}
      <SolicitudTable
        solicitudes={solicitudes}
        loading={loading}
        mostrarDescargaConstancia
      />
    </section>
  )
}
