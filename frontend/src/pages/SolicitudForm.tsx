/**
 * Formulario para crear una nueva solicitud de apoyo económico.
 */

import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { Estudiante, PagedResult, Solicitud, TipoApoyo } from '../types'

/** GUID vacío enviado cuando el backend resuelve el estudiante desde el JWT. */
const ESTUDIANTE_ID_PLACEHOLDER = '00000000-0000-0000-0000-000000000000'

/**
 * Permite a asesores crear solicitudes para cualquier estudiante
 * y a estudiantes crear solicitudes propias.
 */
export function SolicitudForm() {
  const { auth } = useAuth()
  const navigate = useNavigate()
  const [estudiantes, setEstudiantes] = useState<Estudiante[]>([])
  const [estudianteId, setEstudianteId] = useState('')
  const [tipoApoyo, setTipoApoyo] = useState<TipoApoyo>('Beca')
  const [montoSolicitado, setMontoSolicitado] = useState(0)
  const [descripcion, setDescripcion] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    if (auth?.rol !== 'Asesor') {
      return
    }

    api.get<PagedResult<Estudiante>>('/api/estudiantes').then((response) => {
      setEstudiantes(response.data.items)
      setEstudianteId(response.data.items[0]?.id ?? '')
    })
  }, [auth?.rol])

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')

    try {
      const { data } = await api.post<Solicitud>('/api/solicitudes', {
        estudianteId: auth?.rol === 'Asesor' ? estudianteId : ESTUDIANTE_ID_PLACEHOLDER,
        tipoApoyo,
        montoSolicitado,
        descripcion,
      })
      navigate(`/solicitudes/${data.id}`)
    } catch {
      setError('No se pudo crear la solicitud. Verifica los campos.')
    }
  }

  return (
    <section className="page narrow">
      <h1>Nueva solicitud</h1>
      <form className="panel form-panel" onSubmit={submit}>
        {auth?.rol === 'Asesor' && (
          <label>
            Estudiante
            <select value={estudianteId} onChange={(event) => setEstudianteId(event.target.value)}>
              {estudiantes.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nombreCompleto}
                </option>
              ))}
            </select>
          </label>
        )}
        <label>
          Tipo de apoyo
          <select
            value={tipoApoyo}
            onChange={(event) => setTipoApoyo(event.target.value as TipoApoyo)}
          >
            <option value="Beca">Beca</option>
            <option value="Credito">Crédito</option>
            <option value="Subsidio">Subsidio</option>
          </select>
        </label>
        <label>
          Monto solicitado
          <input
            type="number"
            min="1"
            value={montoSolicitado}
            onChange={(event) => setMontoSolicitado(Number(event.target.value))}
          />
        </label>
        <label>
          Descripción
          <textarea value={descripcion} onChange={(event) => setDescripcion(event.target.value)} />
        </label>
        {error && <p className="error">{error}</p>}
        <button>Crear solicitud</button>
      </form>
    </section>
  )
}
