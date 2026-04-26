import RichTextEditor from './RichTextEditor'

interface Props {
  value: string
  onChange: (html: string) => void
  onClose: () => void
}

export default function ScriptEditorModal({ value, onChange, onClose }: Props) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-6">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />

      {/* Modal container */}
      <div
        className="relative z-10 bg-white rounded-xl shadow-2xl flex flex-col w-[90vw] max-w-4xl"
        style={{ maxHeight: '85vh' }}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3 border-b border-gray-200 shrink-0">
          <span className="text-sm font-semibold text-gray-800">Script Content Editor</span>
          <button
            type="button"
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-xl leading-none transition-colors"
          >
            ×
          </button>
        </div>

        {/* Body — scrollable if content is tall */}
        <div className="flex-1 overflow-y-auto p-5">
          <RichTextEditor value={value} onChange={onChange} />
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-2 px-5 py-3 border-t border-gray-200 shrink-0">
          <button
            type="button"
            onClick={onClose}
            className="bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium px-6 py-1.5 rounded-lg transition-colors"
          >
            Done
          </button>
        </div>
      </div>
    </div>
  )
}
