import { Handle, Position } from '@xyflow/react'
import type { HubionNodeType } from '../../../types/designer'
import { NODE_META } from '../../../types/designer'

interface NodeShellProps {
  type: HubionNodeType
  label: string
  isEntry?: boolean
  selected?: boolean
  children?: React.ReactNode
}

export default function NodeShell({ type, label, isEntry, selected, children }: NodeShellProps) {
  const meta = NODE_META[type]
  const hasSingle = meta.handles === 'single'
  const hasDual = meta.handles === 'dual'

  return (
    <div
      style={{
        width: 210,
        borderColor: selected ? meta.color : '#d1d5db',
        borderWidth: selected ? 2 : 1,
      }}
      className="bg-white rounded-lg border shadow-sm overflow-hidden"
    >
      {/* Target handle */}
      <Handle type="target" position={Position.Top} style={{ background: '#9ca3af' }} />

      {/* Colored header */}
      <div
        style={{ backgroundColor: meta.color }}
        className="flex items-center justify-between px-3 py-1.5"
      >
        <span className="text-white text-xs font-semibold uppercase tracking-wide">
          {meta.label}
        </span>
        {isEntry && (
          <span className="bg-white/30 text-white text-[10px] font-bold px-1.5 py-0.5 rounded">
            ENTRY
          </span>
        )}
      </div>

      {/* Body */}
      <div className="px-3 py-2">
        <p className="text-sm font-medium text-gray-800 truncate">{label}</p>
        {children}
      </div>

      {/* Single source handle */}
      {hasSingle && (
        <Handle type="source" position={Position.Bottom} id="default" style={{ background: '#9ca3af' }} />
      )}

      {/* Dual source handles (branch / api_call) */}
      {hasDual && (
        <>
          <Handle
            type="source"
            position={Position.Bottom}
            id={type === 'branch' ? 'true' : 'success'}
            style={{ left: '30%', background: '#22c55e' }}
          />
          <Handle
            type="source"
            position={Position.Bottom}
            id={type === 'branch' ? 'false' : 'error'}
            style={{ left: '70%', background: '#ef4444' }}
          />
        </>
      )}
    </div>
  )
}
