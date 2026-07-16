/**
 * Contexto de autenticación: gestiona la sesión del usuario y expone login/logout.
 */

import { createContext, useContext, useState } from 'react'
import type { ReactNode } from 'react'
import { SESSION_STORAGE_KEY } from '../api/client'
import type { AuthResponse } from '../types'

type AuthContextValue = {
  auth: AuthResponse | null
  login: (auth: AuthResponse) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue>({
  auth: null,
  login: () => undefined,
  logout: () => undefined,
})

/**
 * Lee la sesión persistida en localStorage, si existe.
 */
function leerSesionGuardada(): AuthResponse | null {
  const session = localStorage.getItem(SESSION_STORAGE_KEY)
  return session ? (JSON.parse(session) as AuthResponse) : null
}

type AuthProviderProps = {
  children: ReactNode
}

/**
 * Proveedor que envuelve la aplicación y mantiene el estado de autenticación.
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [auth, setAuth] = useState<AuthResponse | null>(leerSesionGuardada)

  function login(nextAuth: AuthResponse) {
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(nextAuth))
    setAuth(nextAuth)
  }

  function logout() {
    localStorage.removeItem(SESSION_STORAGE_KEY)
    setAuth(null)
  }

  return (
    <AuthContext.Provider value={{ auth, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

/**
 * Hook para acceder al contexto de autenticación desde cualquier componente.
 */
export function useAuth(): AuthContextValue {
  return useContext(AuthContext)
}
