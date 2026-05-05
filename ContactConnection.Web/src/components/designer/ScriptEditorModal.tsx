import { useRef } from 'react'
import RichTextEditor, { type RichTextEditorHandle } from './RichTextEditor'
import VariablePanel from './VariablePanel'

interface Props {
  value: string
  onChange: (html: string) => void
  onClose: () => void
}

export default function ScriptEditorModal({ value, onChange, onClose }: Props) {
  const editorRef = useRef<RichTextEditorHandle>(null)

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-6">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />

      {/* Modal container */}
      <div
        className="relative z-10 bg-white rounded-xl shadow-2xl flex flex-col w-[90vw] max-w-5xl"
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

        {/* Body — editor + variable panel side by side */}
        <div className="flex flex-1 overflow-hidden">
          {/* Editor */}
          <div className="flex-1 overflow-y-auto p-5">
            <RichTextEditor ref={editorRef} value={value} onChange={onChange} />
          </div>

          {/* Variable panel */}
          <div className="w-52 shrink-0 border-l border-gray-200 overflow-y-auto bg-gray-50">
            <VariablePanel onInsert={(token) => editorRef.current?.insert(token)} />
          </div>
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
