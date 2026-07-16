/**
 * Layout principal con barra de navegación para usuarios autenticados.
 */

import { NavLink } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthContext'

/**
 * Props del componente {@link AppShell}.
 */
type AppShellProps = {
  /** Contenido de la página protegida que se renderiza debajo de la barra superior. */
  children: ReactNode
}

/**
 * Envuelve las páginas protegidas con la barra superior y enlaces según el rol.
 *
 * @param props - Propiedades del componente.
 * @param props.children - Página o vista hija a mostrar dentro del layout.
 *
 * Estado consumido del contexto de autenticación:
 * - `auth.nombreCompleto` y `auth.rol` — se muestran en la barra de usuario.
 * - `logout` — cierra sesión y elimina el token de `localStorage`.
 */
export function AppShell({ children }: AppShellProps) {
  const { auth, logout } = useAuth()

  return (
    <main className="app-shell">
      <nav className="topbar" aria-label="Principal">
        <NavLink className="brand" to="/">
          EduApoyos
        </NavLink>
        <div className="nav-links">
          {auth?.rol === 'Asesor' && (
            <NavLink to="/asesor" className={({ isActive }) => (isActive ? 'active' : undefined)}>
              Solicitudes
            </NavLink>
          )}
          {auth?.rol === 'Estudiante' && (
            <NavLink to="/portal" className={({ isActive }) => (isActive ? 'active' : undefined)}>
              Mi portal
            </NavLink>
          )}
          <NavLink
            to="/solicitudes/nueva"
            className={({ isActive }) => (isActive ? 'active' : undefined)}
          >
            Nueva solicitud
          </NavLink>
          <div className="nav-user">
            <div className="nav-user-meta">
              <strong>{auth?.nombreCompleto}</strong>
              <span>{auth?.rol}</span>
            </div>
            <button type="button" className="link-button" onClick={logout}>
              Salir
            </button>
          </div>
        </div>
      </nav>
      {children}
    </main>
  )
}
