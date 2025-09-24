import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { AuthProvider } from './auth/AuthContext'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import Login from './pages/Login'
import Apartments from './pages/Apartments'
import ProtectedRoute from './auth/ProtectedRoute'

const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/', element: <ProtectedRoute><Apartments /></ProtectedRoute> },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  </StrictMode>,
)
