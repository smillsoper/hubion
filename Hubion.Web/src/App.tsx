import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './stores/authStore'
import LoginPage from './pages/LoginPage'
import AgentPage from './pages/AgentPage'
import FlowDesignerPage from './pages/FlowDesignerPage'

function RequireAuth({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.token)
  return token ? <>{children}</> : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/agent"
          element={
            <RequireAuth>
              <AgentPage />
            </RequireAuth>
          }
        />
        <Route
          path="/designer"
          element={
            <RequireAuth>
              <FlowDesignerPage />
            </RequireAuth>
          }
        />
        <Route
          path="/designer/:id"
          element={
            <RequireAuth>
              <FlowDesignerPage />
            </RequireAuth>
          }
        />
        <Route path="*" element={<Navigate to="/agent" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
