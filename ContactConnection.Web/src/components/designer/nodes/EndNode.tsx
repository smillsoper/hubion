import type { NodeProps } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function EndNode({ data, selected }: NodeProps & { data: NodeData }) {
  return (
    <NodeShell type="end" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      <p className="text-xs text-gray-500 mt-0.5">
        status: <span className="font-medium">{(data.status as string) || 'complete'}</span>
      </p>
    </NodeShell>
  )
}
