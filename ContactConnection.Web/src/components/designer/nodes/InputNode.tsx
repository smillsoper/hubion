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

  return (
    <NodeShell type="input" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      <p className="text-xs text-gray-500 mt-0.5">
        {fieldType}
        {(data.required as boolean) && ' · required'}
      </p>

      {/* Output variable badge */}
      {outputVariable && (
        <p className="text-[10px] text-emerald-600 mt-0.5 font-mono truncate">
          → {'{{flow.' + outputVariable + '}}'}
        </p>
      )}

      {/* Option chips shown in node body */}
      {isSelect && (
        <div className="mt-1.5 flex flex-wrap gap-1">
          {options.map((opt) => (
            <span
              key={opt}
              className="text-[10px] bg-emerald-50 text-emerald-700 px-1.5 py-0.5 rounded border border-emerald-200 leading-tight"
            >
              {opt}
            </span>
          ))}
        </div>
      )}

      {/* Source handles — one per option for select, single default otherwise */}
      {isSelect ? (
        options.map((opt, i) => (
          <Handle
            key={opt}
            type="source"
            position={Position.Bottom}
            id={opt}
            title={opt}
            style={{
              left: `${((i + 1) / (options.length + 1)) * 100}%`,
              background: '#10b981',
            }}
          />
        ))
      ) : (
        <Handle
          type="source"
          position={Position.Bottom}
          id="default"
          style={{ background: '#9ca3af' }}
        />
      )}
    </NodeShell>
  )
}
