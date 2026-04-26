import type { NodeProps } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function SetVariableNode({ data, selected }: NodeProps & { data: NodeData }) {
  const assignments = (data.assignments as { variable: string; value: string }[]) ?? []
  return (
    <NodeShell type="set_variable" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      {assignments.length > 0 && assignments[0].variable ? (
        <p className="text-xs text-gray-500 mt-0.5 truncate">
          {assignments.length} assignment{assignments.length !== 1 ? 's' : ''}
        </p>
      ) : (
        <p className="text-xs text-gray-400 mt-0.5 italic">No assignments</p>
      )}
    </NodeShell>
  )
}
