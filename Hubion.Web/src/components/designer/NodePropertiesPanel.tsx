import type { Node } from '@xyflow/react'
import type { NodeData, HubionNodeType } from '../../types/designer'
import ScriptContentEditor from './ScriptContentEditor'

interface Props {
  node: Node<NodeData>
  isEntry: boolean
  onUpdate: (id: string, data: Partial<NodeData>) => void
  onSetEntry: (id: string) => void
  onDelete: (id: string) => void
  onClose: () => void
}

export default function NodePropertiesPanel({
  node,
  isEntry,
  onUpdate,
  onSetEntry,
  onDelete,
  onClose,
}: Props) {
  const type = node.type as HubionNodeType
  const data = node.data

  function field(
    key: keyof NodeData,
    label: string,
    element: React.ReactNode,
  ) {
    return (
      <div key={key as string} className="flex flex-col gap-1">
        <label className="text-xs font-medium text-gray-600">{label}</label>
        {element}
      </div>
    )
  }

  function input(key: keyof NodeData, placeholder = '') {
    return (
      <input
        className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-blue-400"
        value={(data[key] as string) ?? ''}
        placeholder={placeholder}
        onChange={(e) => onUpdate(node.id, { [key]: e.target.value })}
      />
    )
  }

  function textarea(key: keyof NodeData, rows = 3, placeholder = '') {
    return (
      <textarea
        className="w-full border border-gray-300 rounded px-2 py-1 text-sm font-mono focus:outline-none focus:border-blue-400 resize-none"
        rows={rows}
        value={(data[key] as string) ?? ''}
        placeholder={placeholder}
        onChange={(e) => onUpdate(node.id, { [key]: e.target.value })}
      />
    )
  }

  function typeSpecificFields() {
    switch (type) {
      case 'script':
        return (
          <ScriptContentEditor
            key={node.id}
            content={(data.content as string) ?? ''}
            onUpdate={(html) => onUpdate(node.id, { content: html })}
          />
        )

      case 'input':
        return (
          <>
            {field(
              'fieldType',
              'Field Type',
              <select
                className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-blue-400"
                value={(data.fieldType as string) ?? 'text'}
                onChange={(e) => onUpdate(node.id, { fieldType: e.target.value })}
              >
                {['text', 'select', 'checkbox', 'date', 'phone', 'email', 'address'].map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>,
            )}
            {(data.fieldType as string) === 'select' &&
              field('options', 'Options (comma-separated)', textarea('options', 2, 'Option A, Option B, Option C'))}
            <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
              <input
                type="checkbox"
                checked={(data.required as boolean) ?? false}
                onChange={(e) => onUpdate(node.id, { required: e.target.checked })}
              />
              Required
            </label>
          </>
        )

      case 'branch':
        return (
          <>
            {field('condition', 'Condition', input('condition', '{{input.node_id}} == value'))}
            <p className="text-[10px] text-gray-400">Operators: == != &gt; &lt; &gt;= &lt;= contains</p>
            <div className="flex justify-between text-xs mt-1">
              <span className="text-green-600 font-medium">→ true transition</span>
              <span className="text-red-500 font-medium">→ false transition</span>
            </div>
          </>
        )

      case 'set_variable': {
        const assignments = (data.assignments as { variable: string; value: string }[]) ?? [{ variable: '', value: '' }]
        return (
          <div className="flex flex-col gap-2">
            <label className="text-xs font-medium text-gray-600">Assignments</label>
            {assignments.map((a, i) => (
              <div key={i} className="flex gap-1 items-center">
                <input
                  className="flex-1 border border-gray-300 rounded px-2 py-1 text-xs focus:outline-none focus:border-blue-400"
                  placeholder="{{flow.var}}"
                  value={a.variable}
                  onChange={(e) => {
                    const next = [...assignments]
                    next[i] = { ...a, variable: e.target.value }
                    onUpdate(node.id, { assignments: next })
                  }}
                />
                <span className="text-gray-400 text-xs">=</span>
                <input
                  className="flex-1 border border-gray-300 rounded px-2 py-1 text-xs focus:outline-none focus:border-blue-400"
                  placeholder="value"
                  value={a.value}
                  onChange={(e) => {
                    const next = [...assignments]
                    next[i] = { ...a, value: e.target.value }
                    onUpdate(node.id, { assignments: next })
                  }}
                />
                <button
                  className="text-gray-400 hover:text-red-500 text-xs px-1"
                  onClick={() => onUpdate(node.id, { assignments: assignments.filter((_, j) => j !== i) })}
                >×</button>
              </div>
            ))}
            <button
              className="text-xs text-blue-600 hover:text-blue-700 self-start"
              onClick={() => onUpdate(node.id, { assignments: [...assignments, { variable: '', value: '' }] })}
            >+ Add assignment</button>
          </div>
        )
      }

      case 'api_call':
        return (
          <>
            {field(
              'method',
              'Method',
              <select
                className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-blue-400"
                value={(data.method as string) ?? 'GET'}
                onChange={(e) => onUpdate(node.id, { method: e.target.value })}
              >
                {['GET', 'POST', 'PUT', 'PATCH', 'DELETE'].map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>,
            )}
            {field('url', 'URL', input('url', 'https://api.example.com/{{flow.id}}'))}
            {field('headers', 'Headers (JSON)', textarea('headers', 2, '{"Authorization": "Bearer {{flow.token}}"}'))}
            {field('body', 'Body (JSON)', textarea('body', 3, '{"key": "{{input.node_id}}"}'))}
            <p className="text-[10px] text-gray-400">Use {'{{namespace.field}}'} in all fields</p>
          </>
        )

      case 'end':
        return field('status', 'Status', input('status', 'complete'))
    }
  }

  return (
    <div className="w-72 bg-white border-l border-gray-200 flex flex-col overflow-y-auto">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
        <span className="text-sm font-semibold text-gray-800">Node Properties</span>
        <button
          className="text-gray-400 hover:text-gray-600 text-lg leading-none"
          onClick={onClose}
        >×</button>
      </div>

      {/* Fields */}
      <div className="flex flex-col gap-4 px-4 py-4 flex-1">
        {/* Node ID */}
        <div>
          <p className="text-[10px] text-gray-400 font-mono">ID: {node.id}</p>
        </div>

        {/* Label */}
        {field('label', 'Label', input('label', 'Node label'))}

        {/* Type-specific fields */}
        {typeSpecificFields()}
      </div>

      {/* Footer actions */}
      <div className="border-t border-gray-200 px-4 py-3 flex flex-col gap-2">
        {!isEntry && (
          <button
            className="w-full text-sm text-blue-600 hover:text-blue-700 border border-blue-200 hover:border-blue-300 rounded py-1.5 transition-colors"
            onClick={() => onSetEntry(node.id)}
          >
            Set as Entry Node
          </button>
        )}
        {isEntry && (
          <p className="text-xs text-center text-green-600 font-medium">✓ Entry Node</p>
        )}
        <button
          className="w-full text-sm text-red-500 hover:text-red-600 border border-red-200 hover:border-red-300 rounded py-1.5 transition-colors"
          onClick={() => onDelete(node.id)}
        >
          Delete Node
        </button>
      </div>
    </div>
  )
}
