/**
 * Guard de rutas: exige sesión activa y rol autorizado antes de renderizar hijos.
 */

import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthContext'
import { AppShell } from './AppShell'
import type { RolUsuario } from '../types'

/**
 * Props del componente {@link ProtectedRoute}.
 */
type ProtectedRouteProps = {
  /** Roles permitidos para acceder a la ruta. Si el rol del usuario no está incluido, redirige a `/`. */
  roles: RolUsuario[]
  /** Contenido protegido que se envuelve con {@link AppShell} cuando la autorización es válida. */
  children: ReactNode
}

/**
 * Redirige a login si no hay sesión, o al inicio si el rol no coincide.
 *
 * @param props - Propiedades del componente.
 * @param props.roles - Lista blanca de roles autorizados para la ruta actual.
 * @param props.children - Vista hija a renderizar tras validar sesión y rol.
 */
export function ProtectedRoute({ roles, children }: ProtectedRouteProps) {
  const { auth } = useAuth()

  if (!auth) {
    return <Navigate to="/login" replace />
  }

  if (!roles.includes(auth.rol)) {
    return <Navigate to="/" replace />
  }

  return <AppShell>{children}</AppShell>
}
