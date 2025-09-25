import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { AuthProvider } from './auth/AuthContext'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Apartments from './pages/Apartments'
import ProtectedRoute from './auth/ProtectedRoute'
import Clients from './pages/Clients'
import Sales from './pages/Sales'
import Reservations from './pages/Reservations'

const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/', element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
  { path: '/dashboard', element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
  { path: '/apartments', element: <ProtectedRoute><Apartments /></ProtectedRoute> },
  { path: '/clients', element: <ProtectedRoute><Clients /></ProtectedRoute> },
  { path: '/reservations', element: <ProtectedRoute><Reservations /></ProtectedRoute> },
  { path: '/sales', element: <ProtectedRoute><Sales /></ProtectedRoute> },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  </StrictMode>,
)
