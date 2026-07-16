/**
 * Cliente HTTP centralizado para comunicarse con la API de EduApoyos.
 * Inyecta automáticamente el token JWT almacenado en la sesión local.
 */

import axios from 'axios'
import type { AuthResponse } from '../types'

/** Clave usada en localStorage para persistir la sesión del usuario. */
export const SESSION_STORAGE_KEY = 'educapoyos.session'

/**
 * Instancia de Axios configurada con la URL base de la API.
 * La variable VITE_API_URL puede apuntar al backend en desarrollo o producción.
 */
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '',
})

/**
 * Interceptor de peticiones: adjunta el Bearer token si existe sesión activa.
 */
api.interceptors.request.use((config) => {
  const session = localStorage.getItem(SESSION_STORAGE_KEY)
  if (session) {
    const auth = JSON.parse(session) as AuthResponse
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  return config
})
