import { useState } from 'react'
import type { Node, Edge } from '@xyflow/react'
import RichTextEditor from './RichTextEditor'
import ScriptEditorModal from './ScriptEditorModal'
import type { NodeData } from '../../types/designer'

interface Props {
  content: string
  onUpdate: (html: string) => void
  dark?: boolean
  /** Flow context — passed through to the modal's variable panel. */
  nodeId?: string
  nodes?: Node<NodeData>[]
  edges?: Edge[]
  entryNodeId?: string | null
}

export default function ScriptContentEditor({
  content, onUpdate, dark,
  nodeId, nodes, edges, entryNodeId,
}: Props) {
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <div className="flex flex-col gap-1">
      <label className={`text-xs font-medium ${dark ? 'text-gray-400' : 'text-gray-600'}`}>Script Content</label>

      {modalOpen ? (
        <>
          <div className={`border border-dashed rounded px-3 py-2 min-h-28 flex items-center justify-center text-xs select-none ${dark ? 'border-gray-700 text-gray-500' : 'border-gray-200 text-gray-400'}`}>
            Editing in expanded view…
          </div>
          <ScriptEditorModal
            value={content}
            onChange={onUpdate}
            onClose={() => setModalOpen(false)}
            nodeId={nodeId}
            nodes={nodes}
            edges={edges}
            entryNodeId={entryNodeId}
          />
        </>
      ) : (
        <RichTextEditor
          value={content}
          onChange={onUpdate}
          onExpand={() => setModalOpen(true)}
          dark={dark}
        />
      )}
    </div>
  )
}
