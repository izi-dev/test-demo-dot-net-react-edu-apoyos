/**
 * Página de inicio de sesión con credenciales de demostración.
 */

import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { AuthResponse } from '../types'

/**
 * Formulario de login que redirige según el rol del usuario autenticado.
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
          <span>Constancias en PDF</span>
        </div>
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
    </main>
  )
}
