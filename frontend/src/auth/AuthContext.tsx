/**
 * Contexto de autenticación: gestiona la sesión del usuario y expone login/logout.
 */

import { createContext, useContext, useState } from 'react'
import type { ReactNode } from 'react'
import { SESSION_STORAGE_KEY } from '../api/client'
import type { AuthResponse } from '../types'

/**
 * Valor expuesto por el contexto de autenticación.
 */
type AuthContextValue = {
  /** Datos de la sesión activa; `null` si el usuario no ha iniciado sesión. */
  auth: AuthResponse | null
  /**
   * Persiste la sesión en `localStorage` y actualiza el estado global.
   * @param auth - Respuesta del endpoint de login con token y datos del usuario.
   */
  login: (auth: AuthResponse) => void
  /** Elimina la sesión de `localStorage` y limpia el estado global. */
  logout: () => void
}

const AuthContext = createContext<AuthContextValue>({
  auth: null,
  login: () => undefined,
  logout: () => undefined,
})

/**
 * Lee la sesión persistida en localStorage, si existe.
 *
 * @returns Objeto {@link AuthResponse} parseado o `null` si no hay sesión guardada.
 */
function leerSesionGuardada(): AuthResponse | null {
  const session = localStorage.getItem(SESSION_STORAGE_KEY)
  return session ? (JSON.parse(session) as AuthResponse) : null
}

/**
 * Props del componente {@link AuthProvider}.
 */
type AuthProviderProps = {
  /** Árbol de componentes que tendrán acceso al contexto de autenticación. */
  children: ReactNode
}

/**
 * Proveedor que envuelve la aplicación y mantiene el estado de autenticación.
 *
 * Estado interno:
 * - `auth` — inicializado desde `localStorage` al montar el componente.
 *
 * @param props - Propiedades del proveedor.
 * @param props.children - Componentes descendientes que consumen `useAuth`.
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
 *
 * @returns Valor del contexto con `auth`, `login` y `logout`.
 */
export function useAuth(): AuthContextValue {
  return useContext(AuthContext)
}
