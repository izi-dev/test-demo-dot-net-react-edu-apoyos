/**
 * Página de inicio de sesión con credenciales de demostración
 * y la trayectoria profesional del autor del proyecto.
 */

import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import {
  AUTOR_NOMBRE,
  AUTOR_ROL,
  TRAYECTORIA_HITOS,
  TRAYECTORIA_PARRAFOS,
} from '../content/trayectoria'
import type { AuthResponse } from '../types'

/**
 * Formulario de login que redirige según el rol del usuario autenticado.
 * Debajo del acceso muestra la sección «Sobre mí» con la trayectoria profesional.
 *
 * Estado del formulario:
 * - `email` — correo institucional (valor inicial: demo asesor).
 * - `password` — contraseña del usuario.
 * - `error` — mensaje de error de autenticación.
 * - `loading` — indica envío en curso al API.
 *
 * Validaciones HTML:
 * - `email` — campo requerido con tipo `email`.
 * - `password` — campo requerido.
 *
 * Llamadas API:
 * - `POST /api/auth/login` — envía `{ email, password }` y recibe {@link AuthResponse}.
 *
 * Tras login exitoso redirige a `/asesor` (rol Asesor) o `/portal` (rol Estudiante).
 */
export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('asesor@educapoyos.local')
  const [password, setPassword] = useState('Asesor123*')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    setLoading(true)
    setError('')

    try {
      const { data } = await api.post<AuthResponse>('/api/auth/login', { email, password })
      login(data)
      navigate(data.rol === 'Asesor' ? '/asesor' : '/portal')
    } catch {
      setError('No fue posible iniciar sesión. Revisa tu correo y contraseña.')
    } finally {
      setLoading(false)
    }
  }

  function usarCredencialesAsesor() {
    setEmail('asesor@educapoyos.local')
    setPassword('Asesor123*')
    setError('')
  }

  function usarCredencialesEstudiante() {
    setEmail('estudiante@educapoyos.local')
    setPassword('Estudiante123*')
    setError('')
  }

  return (
    <main className="login-page">
      <div className="login-viewport">
        <section className="login-hero">
          <p className="eyebrow">Institución educativa</p>
          <h1 className="brand-mark">EduApoyos</h1>
          <p className="lede">
            Solicita, revisa y da seguimiento a becas, créditos y subsidios con trazabilidad completa
            del proceso.
          </p>
          <div className="hero-meta">
            <span>Flujo guiado por roles</span>
            <span>Historial de estados</span>
            <span>Constancias descargables</span>
          </div>
          <a className="hero-about-link" href="#sobre-mi">
            Sobre el autor →
          </a>
        </section>

        <form className="login-form" onSubmit={submit} noValidate>
          <h2>Iniciar sesión</h2>
          <p className="form-hint">Ingresa con tu cuenta institucional para continuar.</p>

          <label>
            Correo electrónico
            <input
              type="email"
              autoComplete="username"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>

          <label>
            Contraseña
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>

          {error && <p className="error">{error}</p>}

          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Verificando...' : 'Entrar'}
          </button>

          <div className="demo-users" aria-label="Cuentas de demostración">
            <button type="button" onClick={usarCredencialesAsesor}>
              Demo asesor
            </button>
            <button type="button" onClick={usarCredencialesEstudiante}>
              Demo estudiante
            </button>
          </div>
        </form>
      </div>

      <section id="sobre-mi" className="about-section" aria-labelledby="about-title">
        <div className="about-inner">
          <header className="about-header">
            <p className="eyebrow">Sobre mí</p>
            <h2 id="about-title">{AUTOR_NOMBRE}</h2>
            <p className="about-role">{AUTOR_ROL}</p>
            <p className="about-lead">
              Autor de esta demo técnica (EduApoyos). Más de una década construyendo software para
              startups, banca, fintech, salud, educación y sector público.
            </p>
          </header>

          <ul className="about-hitos" aria-label="Empresas y etapas">
            {TRAYECTORIA_HITOS.map((hito) => (
              <li key={hito}>{hito}</li>
            ))}
          </ul>

          <article className="about-story">
            <h3>Mi trayectoria profesional</h3>
            {TRAYECTORIA_PARRAFOS.map((parrafo) => (
              <p key={parrafo.slice(0, 48)}>{parrafo}</p>
            ))}
          </article>
        </div>
      </section>
    </main>
  )
}
