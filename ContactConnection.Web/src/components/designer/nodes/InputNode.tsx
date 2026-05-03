import type { NodeProps } from '@xyflow/react'
import NodeShell from './NodeShell'
import type { NodeData } from '../../../types/designer'

export default function InputNode({ data, selected }: NodeProps & { data: NodeData }) {
  return (
    <NodeShell type="input" label={data.label as string} isEntry={data.isEntry as boolean} selected={selected}>
      <p className="text-xs text-gray-500 mt-0.5">
        {(data.fieldType as string) ?? 'text'}
        {(data.required as boolean) && ' · required'}
      </p>
    </NodeShell>
  )
}
