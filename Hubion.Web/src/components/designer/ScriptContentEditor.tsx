import { useState } from 'react'
import RichTextEditor from './RichTextEditor'
import ScriptEditorModal from './ScriptEditorModal'

interface Props {
  content: string
  onUpdate: (html: string) => void
}

export default function ScriptContentEditor({ content, onUpdate }: Props) {
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <div className="flex flex-col gap-1">
      <label className="text-xs font-medium text-gray-600">Script Content</label>

      {modalOpen ? (
        <>
          {/* Placeholder while modal is open so the panel keeps its shape */}
          <div className="border border-dashed border-gray-200 rounded px-3 py-2 min-h-28 flex items-center justify-center text-xs text-gray-400 select-none">
            Editing in expanded view…
          </div>
          <ScriptEditorModal
            value={content}
            onChange={onUpdate}
            onClose={() => setModalOpen(false)}
          />
        </>
      ) : (
        <RichTextEditor
          value={content}
          onChange={onUpdate}
          onExpand={() => setModalOpen(true)}
        />
      )}
    </div>
  )
}
