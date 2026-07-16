/**
 * Formulario para crear una nueva solicitud de apoyo económico.
 */

import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { Estudiante, PagedResult, Solicitud, TipoApoyo } from '../types'

/**
 * GUID vacío enviado cuando el backend resuelve el estudiante desde el JWT del usuario en sesión.
 */
const ESTUDIANTE_ID_PLACEHOLDER = '00000000-0000-0000-0000-000000000000'

/**
 * Permite a asesores crear solicitudes para cualquier estudiante
 * y a estudiantes crear solicitudes propias.
 *
 * Estado del formulario:
 * - `estudiantes` — listado cargado solo para rol Asesor.
 * - `cargandoEstudiantes` — indica carga del selector de estudiantes.
 * - `estudianteId` — estudiante seleccionado (solo Asesor).
 * - `tipoApoyo` — tipo de apoyo (`Beca`, `Credito` o `Subsidio`).
 * - `montoSolicitado` — monto en COP como cadena del input numérico.
 * - `descripcion` — justificación de la solicitud.
 * - `error` — mensaje de validación o error del API.
 * - `enviando` — indica envío en curso.
 *
 * Validaciones (cliente):
 * - Monto: debe ser un número finito mayor a cero.
 * - Descripción: mínimo 10 caracteres tras recortar espacios.
 * - Estudiante: obligatorio si el rol es Asesor.
 * - HTML: campos `required` en estudiante (asesor), monto y descripción.
 *
 * Llamadas API:
 * - `GET /api/estudiantes` — carga estudiantes paginados (solo Asesor).
 * - `POST /api/solicitudes` — crea la solicitud con `{ estudianteId, tipoApoyo, montoSolicitado, descripcion }`.
 *
 * Tras crear exitosamente navega a `/solicitudes/{id}`.
 */
export function SolicitudForm() {
  const { auth } = useAuth()
  const navigate = useNavigate()
  const [estudiantes, setEstudiantes] = useState<Estudiante[]>([])
  const [cargandoEstudiantes, setCargandoEstudiantes] = useState(auth?.rol === 'Asesor')
  const [estudianteId, setEstudianteId] = useState('')
  const [tipoApoyo, setTipoApoyo] = useState<TipoApoyo>('Beca')
  const [montoSolicitado, setMontoSolicitado] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  useEffect(() => {
    if (auth?.rol !== 'Asesor') {
      return
    }

    setCargandoEstudiantes(true)
    api
      .get<PagedResult<Estudiante>>('/api/estudiantes')
      .then((response) => {
        setEstudiantes(response.data.items)
        setEstudianteId(response.data.items[0]?.id ?? '')
      })
      .catch(() => setError('No se pudieron cargar los estudiantes.'))
      .finally(() => setCargandoEstudiantes(false))
  }, [auth?.rol])

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')

    const monto = Number(montoSolicitado)
    if (!Number.isFinite(monto) || monto <= 0) {
      setError('Indica un monto válido mayor a cero.')
      return
    }

    if (descripcion.trim().length < 10) {
      setError('La descripción debe tener al menos 10 caracteres.')
      return
    }

    if (auth?.rol === 'Asesor' && !estudianteId) {
      setError('Selecciona un estudiante.')
      return
    }

    setEnviando(true)
    try {
      const { data } = await api.post<Solicitud>('/api/solicitudes', {
        estudianteId: auth?.rol === 'Asesor' ? estudianteId : ESTUDIANTE_ID_PLACEHOLDER,
        tipoApoyo,
        montoSolicitado: monto,
        descripcion: descripcion.trim(),
      })
      navigate(`/solicitudes/${data.id}`)
    } catch {
      setError('No se pudo crear la solicitud. Verifica los campos e intenta de nuevo.')
    } finally {
      setEnviando(false)
    }
  }

  const volverA = auth?.rol === 'Asesor' ? '/asesor' : '/portal'

  return (
    <section className="page narrow">
      <Link className="back-link" to={volverA}>
        ← Volver
      </Link>
      <header className="page-header">
        <div>
          <p className="eyebrow">Nueva solicitud</p>
          <h1>Registrar apoyo</h1>
          <p className="support">
            Completa los datos. La solicitud quedará en estado pendiente para revisión.
          </p>
        </div>
      </header>

      <form className="panel form-stack" onSubmit={submit} noValidate>
        {auth?.rol === 'Asesor' && (
          <label>
            Estudiante
            <select
              value={estudianteId}
              onChange={(event) => setEstudianteId(event.target.value)}
              disabled={cargandoEstudiantes || estudiantes.length === 0}
              required
            >
              {cargandoEstudiantes && <option value="">Cargando...</option>}
              {!cargandoEstudiantes && estudiantes.length === 0 && (
                <option value="">No hay estudiantes</option>
              )}
              {estudiantes.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nombreCompleto} · {item.programaAcademico}
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
          <span className="field-hint">Valor en pesos colombianos (COP)</span>
          <input
            type="number"
            min="1"
            step="1"
            inputMode="numeric"
            placeholder="Ej. 2500000"
            value={montoSolicitado}
            onChange={(event) => setMontoSolicitado(event.target.value)}
            required
          />
        </label>

        <label>
          Descripción / justificación
          <span className="field-hint">Explica el motivo de la solicitud (mín. 10 caracteres)</span>
          <textarea
            value={descripcion}
            onChange={(event) => setDescripcion(event.target.value)}
            placeholder="Describe el apoyo requerido y su justificación académica o socioeconómica."
            required
          />
        </label>

        {error && <p className="error">{error}</p>}

        <button type="submit" className="btn btn-primary" disabled={enviando || cargandoEstudiantes}>
          {enviando ? 'Guardando...' : 'Crear solicitud'}
        </button>
      </form>
    </section>
  )
}
