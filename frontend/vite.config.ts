/**
 * Configuración de Vite para el frontend de EduApoyos.
 * Define el plugin de React, el puerto de desarrollo y el proxy hacia la API .NET.
 */

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

/**
 * Exporta la configuración del servidor de desarrollo y del empaquetado.
 *
 * - `plugins`: habilita la transformación JSX/TSX con Fast Refresh.
 * - `server.port`: puerto local del dev server (5173).
 * - `server.proxy['/api']`: reenvía peticiones `/api/*` al backend para evitar CORS en desarrollo.
 *   El destino se toma de `VITE_API_PROXY` o, por defecto, `http://localhost:5000`.
 */
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_API_PROXY ?? 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
