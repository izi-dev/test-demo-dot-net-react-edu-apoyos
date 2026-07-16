/**
 * Punto de entrada de rutas de la aplicación EduApoyos.
 * Solo define el enrutamiento; la lógica vive en páginas y componentes.
 */

import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './auth/AuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AdvisorDashboard } from './pages/AdvisorDashboard'
import { LoginPage } from './pages/LoginPage'
import { SolicitudDetail } from './pages/SolicitudDetail'
import { SolicitudForm } from './pages/SolicitudForm'
import { StudentPortal } from './pages/StudentPortal'
import './App.css'

/**
 * Redirige al panel correspondiente según el rol del usuario autenticado.
 */
function RoleRedirect() {
  const { auth } = useAuth()

  if (!auth) {
    return <Navigate to="/login" replace />
  }

  return <Navigate to={auth.rol === 'Asesor' ? '/asesor' : '/portal'} replace />
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<RoleRedirect />} />
          <Route
            path="/asesor"
            element={
              <ProtectedRoute roles={['Asesor']}>
                <AdvisorDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/solicitudes/nueva"
            element={
              <ProtectedRoute roles={['Asesor', 'Estudiante']}>
                <SolicitudForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/solicitudes/:id"
            element={
              <ProtectedRoute roles={['Asesor', 'Estudiante']}>
                <SolicitudDetail />
              </ProtectedRoute>
            }
          />
          <Route
            path="/portal"
            element={
              <ProtectedRoute roles={['Estudiante']}>
                <StudentPortal />
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
