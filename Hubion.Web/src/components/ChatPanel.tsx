export default function ChatPanel() {
  return (
    <div className="flex flex-col h-full">
      <div className="px-4 py-3 border-b border-gray-800 shrink-0">
        <p className="text-sm font-medium text-gray-300">Team Chat</p>
      </div>
      <div className="flex-1 flex flex-col items-center justify-center gap-3 p-4">
        <div className="w-12 h-12 rounded-full bg-gray-800 flex items-center justify-center">
          <svg
            className="w-6 h-6 text-gray-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={1.5}
              d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 01.865-.501 48.172 48.172 0 003.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0012 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018z"
            />
          </svg>
        </div>
        <p className="text-gray-500 text-xs text-center">Channels, DMs &amp; threads</p>
        <p className="text-gray-600 text-xs text-center">Coming soon</p>
      </div>
    </div>
  )
}
