import type { NodeProps } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function ApiCallNode({ data, selected }: NodeProps & { data: NodeData }) {
  const method = (data.method as string) ?? 'GET'
  const url = (data.url as string) ?? ''
  return (
    <NodeShell type="api_call" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      <p className="text-xs text-gray-500 mt-0.5 truncate">
        <span className="font-mono font-semibold">{method}</span>
        {url ? ` ${url}` : ' — no URL'}
      </p>
      <div className="flex justify-between mt-1.5 text-[10px]">
        <span className="text-green-600 font-medium">✓ success</span>
        <span className="text-red-500 font-medium">✗ error</span>
      </div>
    </NodeShell>
  )
}
