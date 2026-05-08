interface Props {
  secondsLeft: number
  onKeepAlive: () => void
  onLogout: () => void
}

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return m > 0
    ? `${m}:${String(s).padStart(2, '0')}`
    : `${s}s`
}

export default function SessionTimeoutModal({ secondsLeft, onKeepAlive, onLogout }: Props) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-sm mx-4 p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 rounded-full bg-amber-100 flex items-center justify-center shrink-0">
            <svg className="w-5 h-5 text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3m0 3h.01M12 3a9 9 0 100 18A9 9 0 0012 3z" />
            </svg>
          </div>
          <div>
            <h2 className="text-base font-semibold text-gray-900">Session expiring</h2>
            <p className="text-sm text-gray-500">You'll be signed out in</p>
          </div>
        </div>

        <div className="text-center my-6">
          <span className="text-5xl font-mono font-bold text-gray-900 tabular-nums">
            {formatTime(secondsLeft)}
          </span>
        </div>

        <div className="flex flex-col gap-2">
          <button
            onClick={onKeepAlive}
            className="w-full px-4 py-2.5 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Keep me logged in
          </button>
          <button
            onClick={onLogout}
            className="w-full px-4 py-2.5 text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            Sign out now
          </button>
        </div>
      </div>
    </div>
  )
}
