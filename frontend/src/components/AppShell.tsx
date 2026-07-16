/**
 * Layout principal con barra de navegación para usuarios autenticados.
 */

import { Link } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthContext'

type AppShellProps = {
  children: ReactNode
}

/**
 * Envuelve las páginas protegidas con la barra superior y enlaces según el rol.
 */
export function AppShell({ children }: AppShellProps) {
  const { auth, logout } = useAuth()

  return (
    <main className="app-shell">
      <nav className="topbar">
        <Link className="brand" to="/">
          EduApoyos
        </Link>
        <div className="nav-links">
          {auth?.rol === 'Asesor' && <Link to="/asesor">Solicitudes</Link>}
          <Link to="/solicitudes/nueva">Nueva solicitud</Link>
          {auth?.rol === 'Estudiante' && <Link to="/portal">Mi portal</Link>}
          <button type="button" className="link-button" onClick={logout}>
            Salir
          </button>
        </div>
      </nav>
      {children}
    </main>
  )
}
