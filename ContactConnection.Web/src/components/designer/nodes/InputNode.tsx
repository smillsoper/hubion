import type { NodeProps } from '@xyflow/react'
import { Handle, Position } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function InputNode({ data, selected }: NodeProps & { data: NodeData }) {
  const fieldType = (data.fieldType as string) ?? 'text'
  const rawOptions = (data.options as string) ?? ''
  const outputVariable = (data.outputVariable as string) ?? ''
  const options = fieldType === 'select'
    ? rawOptions.split(',').map((o) => o.trim()).filter(Boolean)
    : []
  const isSelect = options.length > 0

  // Handles sit inside the option-chip strip. The strip is ~26 px tall at the bottom of the card;
  // bottom: 13px places the handle center in the vertical middle of that strip.
  const sourceHandles = isSelect ? (
    <>
      {options.map((opt, i) => (
        <Handle
          key={opt}
          type="source"
          position={Position.Bottom}
          id={opt}
          title={opt}
          style={{
            bottom: 13,
            left: `${((i + 0.5) / options.length) * 100}%`,
            transform: 'translate(-50%, 50%)',
            background: '#10b981',
            zIndex: 10,
          }}
        />
      ))}
    </>
  ) : (
    <Handle type="source" position={Position.Bottom} id="default" style={{ background: '#9ca3af' }} />
  )

  const scriptLabel = (data.scriptLabel as string) ?? ''
  const scriptContent = (data.scriptContent as string) ?? ''
  const hasScript = scriptLabel || scriptContent

  return (
    <NodeShell
      type="input"
      label={data.label as string}
      isEntry={data.isEntry as boolean}
      selected={selected}
      sourceHandles={sourceHandles}
    >
      {hasScript && (
        <p className="text-[10px] text-sky-400 mt-0.5 font-medium truncate">
          📄 {scriptLabel || 'Script attached'}
        </p>
      )}
      <p className="text-xs text-gray-400 mt-0.5">
        {fieldType}
        {(data.required as boolean) && ' · required'}
      </p>

      {/* Output variable badge */}
      {outputVariable && (
        <p className="text-[10px] text-emerald-400 mt-0.5 font-mono truncate">
          → {'{{flow.' + outputVariable + '}}'}
        </p>
      )}

      {/* Option chip strip — full-width bottom section so chips align with handle positions */}
      {isSelect && (
        <div className="-mx-3 -mb-2 mt-2 border-t border-gray-700 flex">
          {options.map((opt) => (
            <div
              key={opt}
              className="flex-1 text-center py-1.5 text-[10px] text-gray-400 font-medium border-r border-gray-700 last:border-r-0 truncate px-1"
            >
              {opt}
            </div>
          ))}
        </div>
      )}
    </NodeShell>
  )
}
