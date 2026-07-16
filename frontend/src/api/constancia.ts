/**
 * Operaciones relacionadas con la descarga de constancias de solicitudes.
 */

import { api } from './client'

/**
 * Descarga la constancia en texto plano de una solicitud aprobada o en curso.
 * Solo está disponible para el estudiante propietario de la solicitud.
 *
 * @param estudianteId - Identificador del estudiante dueño de la solicitud.
 * @param solicitudId - Identificador de la solicitud a certificar.
 */
export async function descargarConstancia(estudianteId: string, solicitudId: string): Promise<void> {
  const { data } = await api.get<Blob>(
    `/api/estudiantes/${estudianteId}/solicitudes/${solicitudId}/constancia`,
    { responseType: 'blob' },
  )

  const url = window.URL.createObjectURL(data)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = `constancia-${solicitudId}.txt`
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()
  window.URL.revokeObjectURL(url)
}
