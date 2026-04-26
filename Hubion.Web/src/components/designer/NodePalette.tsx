import { NODE_META } from '../../types/designer'
import type { HubionNodeType } from '../../types/designer'

const NODE_TYPES: HubionNodeType[] = [
  'script',
  'input',
  'branch',
  'set_variable',
  'api_call',
  'end',
]

export default function NodePalette() {
  function onDragStart(e: React.DragEvent, type: HubionNodeType) {
    e.dataTransfer.setData('application/reactflow-node-type', type)
    e.dataTransfer.effectAllowed = 'move'
  }

  return (
    <div className="w-44 bg-white border-r border-gray-200 flex flex-col overflow-y-auto">
      <div className="px-3 pt-3 pb-2">
        <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Node Types</p>
      </div>
      <div className="flex flex-col gap-2 px-2 pb-4">
        {NODE_TYPES.map((type) => {
          const meta = NODE_META[type]
          return (
            <div
              key={type}
              draggable
              onDragStart={(e) => onDragStart(e, type)}
              className="cursor-grab active:cursor-grabbing select-none rounded-md border border-gray-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow"
            >
              <div
                style={{ backgroundColor: meta.color }}
                className="px-2 py-1"
              >
                <span className="text-white text-xs font-semibold">{meta.label}</span>
              </div>
              <div className="px-2 py-1.5 bg-white">
                <p className="text-[11px] text-gray-500 leading-tight">{meta.description}</p>
              </div>
            </div>
          )
        })}
      </div>
      <div className="mt-auto px-3 pb-3">
        <p className="text-[10px] text-gray-400 leading-tight">Drag a node type onto the canvas to add it</p>
      </div>
    </div>
  )
}
