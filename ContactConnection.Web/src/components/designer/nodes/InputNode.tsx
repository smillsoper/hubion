import type { NodeProps } from '@xyflow/react'
import { Handle, Position, useEdges, useNodeId } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function InputNode({ data, selected }: NodeProps & { data: NodeData }) {
  const nodeId    = useNodeId()
  const edges     = useEdges()

  const fieldType      = (data.fieldType as string) ?? 'text'
  const rawOptions     = (data.options as string) ?? ''
  const outputVariable = (data.outputVariable as string) ?? ''
  const scriptLabel    = (data.scriptLabel as string) ?? ''
  const scriptContent  = (data.scriptContent as string) ?? ''
  const hasScript      = scriptLabel || scriptContent

  const options = fieldType === 'select'
    ? rawOptions.split(',').map((o) => o.trim()).filter(Boolean)
    : []

  // Which options already have an outgoing edge from this node?
  const wiredOptions = new Set(
    edges.filter((e) => e.source === nodeId).map((e) => e.sourceHandle),
  )
  const missingOptions = options.filter((o) => !wiredOptions.has(o))
  const hasWarning = options.length > 0 && missingOptions.length > 0

  return (
    <NodeShell
      type="input"
      label={data.label as string}
      isEntry={data.isEntry as boolean}
      selected={selected}
      sourceHandles={
        <Handle
          type="source"
          position={Position.Bottom}
          id="default"
          style={{ background: '#9ca3af' }}
        />
      }
    >
      {hasScript && (
        <p className="text-[10px] text-sky-400 mt-0.5 font-medium truncate">
          📄 {scriptLabel || 'Script attached'}
        </p>
      )}

      <p className="text-xs text-gray-400 mt-0.5">
        {fieldType}
        {options.length > 0 && ` · ${options.length} options`}
        {(data.required as boolean) && ' · required'}
      </p>

      {outputVariable && (
        <p className="text-[10px] text-emerald-400 mt-0.5 font-mono truncate">
          → {'{{flow.' + outputVariable + '}}'}
        </p>
      )}

      {hasWarning && (
        <p className="text-[10px] text-amber-400 mt-0.5 font-medium">
          ⚠ {missingOptions.length} option{missingOptions.length > 1 ? 's' : ''} not wired
        </p>
      )}
    </NodeShell>
  )
}
