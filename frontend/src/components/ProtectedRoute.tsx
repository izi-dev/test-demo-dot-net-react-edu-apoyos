/**
 * Guard de rutas: exige sesión activa y rol autorizado antes de renderizar hijos.
 */

import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthContext'
import { AppShell } from './AppShell'
import type { RolUsuario } from '../types'

type ProtectedRouteProps = {
  /** Roles permitidos para acceder a la ruta. */
  roles: RolUsuario[]
  children: ReactNode
}

/**
 * Redirige a login si no hay sesión, o al inicio si el rol no coincide.
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
