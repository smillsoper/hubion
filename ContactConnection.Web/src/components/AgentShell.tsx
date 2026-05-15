import { useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../stores/authStore'
import { useSessionTimeout } from '../hooks/useSessionTimeout'
import SessionTimeoutModal from './SessionTimeoutModal'
import SoftphonePanel from './SoftphonePanel'
import FlowPanel from './FlowPanel'
import ChatPanel from './ChatPanel'

export default function AgentShell() {
  const clearAuth = useAuthStore((s) => s.clearAuth)
  const navigate = useNavigate()

  const handleLogout = useCallback(() => {
    clearAuth()
    navigate('/login', { replace: true })
  }, [clearAuth, navigate])

  const { showWarning, secondsLeft, keepAlive } = useSessionTimeout(handleLogout)

  return (
    <div className="h-screen flex flex-col bg-gray-950 text-white overflow-hidden">
      {showWarning && (
        <SessionTimeoutModal
          secondsLeft={secondsLeft}
          onKeepAlive={keepAlive}
          onLogout={handleLogout}
        />
      )}

      {/* Top bar */}
      <header className="flex items-stretch bg-gray-900 border-b border-gray-800 shrink-0">
        <img src="/cc-navbar-dark.svg" alt="Contact Connection" className="shrink-0 block" />
        <div className="flex items-center justify-end flex-1 gap-4 px-4">
          <button
            onClick={() => navigate('/flows')}
            className="text-xs text-gray-400 hover:text-indigo-300 transition-colors"
          >
            Flows
          </button>
          <button
            onClick={handleLogout}
            className="text-xs text-gray-400 hover:text-white transition-colors"
          >
            Sign out
          </button>
        </div>
      </header>

      {/* 3-panel body */}
      <div className="flex flex-1 overflow-hidden">
        {/* Left — Softphone (~240px) */}
        <div className="w-60 shrink-0 border-r border-gray-800 overflow-y-auto">
          <SoftphonePanel />
        </div>

        {/* Center — Flow/Script (flex grow) */}
        <div className="flex-1 overflow-y-auto">
          <FlowPanel />
        </div>

        {/* Right — Chat (~300px) */}
        <div className="w-75 shrink-0 border-l border-gray-800 overflow-y-auto">
          <ChatPanel />
        </div>
      </div>
    </div>
  )
}
