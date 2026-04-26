import type { NodeProps } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function BranchNode({ data, selected }: NodeProps & { data: NodeData }) {
  return (
    <NodeShell type="branch" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      {data.condition ? (
        <p className="text-xs text-gray-500 mt-0.5 truncate font-mono">{data.condition as string}</p>
      ) : (
        <p className="text-xs text-gray-400 mt-0.5 italic">No condition</p>
      )}
      <div className="flex justify-between mt-1.5 text-[10px] text-gray-400">
        <span className="text-green-600 font-medium">✓ true</span>
        <span className="text-red-500 font-medium">✗ false</span>
      </div>
    </NodeShell>
  )
}
