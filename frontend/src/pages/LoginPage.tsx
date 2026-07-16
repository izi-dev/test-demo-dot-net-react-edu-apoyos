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
      setError('No fue posible iniciar sesión. Revisa tus credenciales.')
    } finally {
      setLoading(false)
    }
  }

  function usarCredencialesAsesor() {
    setEmail('asesor@educapoyos.local')
    setPassword('Asesor123*')
  }

  function usarCredencialesEstudiante() {
    setEmail('estudiante@educapoyos.local')
    setPassword('Estudiante123*')
  }

  return (
    <main className="login-page">
      <section className="login-hero">
        <p className="eyebrow">Gestión de apoyos económicos</p>
        <h1>EduApoyos</h1>
        <p>Registra, revisa y consulta becas, créditos y subsidios con trazabilidad completa.</p>
      </section>
      <form className="panel form-panel" onSubmit={submit}>
        <h2>Ingresar</h2>
        <label>
          Correo
          <input value={email} onChange={(event) => setEmail(event.target.value)} />
        </label>
        <label>
          Contraseña
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button disabled={loading}>{loading ? 'Ingresando...' : 'Entrar'}</button>
        <div className="demo-users">
          <button type="button" onClick={usarCredencialesAsesor}>
            Usar asesor
          </button>
          <button type="button" onClick={usarCredencialesEstudiante}>
            Usar estudiante
          </button>
        </div>
      </form>
    </main>
  )
}
