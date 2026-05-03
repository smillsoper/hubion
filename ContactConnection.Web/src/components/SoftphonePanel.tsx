export default function SoftphonePanel() {
  return (
    <div className="flex flex-col items-center justify-center h-full min-h-64 gap-3 p-4">
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
            d="M2.25 6.75c0 8.284 6.716 15 15 15h2.25a2.25 2.25 0 002.25-2.25v-1.372c0-.516-.351-.966-.852-1.091l-4.423-1.106c-.44-.11-.902.055-1.173.417l-.97 1.293c-.282.376-.769.542-1.21.38a12.035 12.035 0 01-7.143-7.143c-.162-.441.004-.928.38-1.21l1.293-.97c.363-.271.527-.734.417-1.173L6.963 3.102a1.125 1.125 0 00-1.091-.852H4.5A2.25 2.25 0 002.25 4.5v2.25z"
          />
        </svg>
      </div>
      <p className="text-gray-500 text-xs text-center">Softphone</p>
      <p className="text-gray-600 text-xs text-center">Coming soon</p>
    </div>
  )
}
