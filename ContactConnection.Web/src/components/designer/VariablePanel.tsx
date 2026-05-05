import { useState } from 'react'

interface VariableField {
  key: string
  token?: string
}

interface VariableNamespace {
  ns: string
  fields: VariableField[]
  dynamic?: boolean
  dynamicNote?: string
}

const NAMESPACES: VariableNamespace[] = [
  {
    ns: 'call_record',
    fields: [
      { key: 'id' },
      { key: 'status' },
      { key: 'call_source' },
      { key: 'record_type' },
      { key: 'phone_number' },
      { key: 'account_number' },
      { key: 'list_id' },
      { key: 'campaign_id' },
      { key: 'disposition' },
      { key: 'notes' },
      { key: 'call_started_at' },
      { key: 'call_ended_at' },
      { key: 'handle_time_seconds' },
    ],
  },
  {
    ns: 'caller',
    fields: [
      { key: 'name' },
      { key: 'first_name' },
      { key: 'last_name' },
      { key: 'phone' },
      { key: 'email' },
      { key: 'account_number' },
      { key: 'billing_address' },
      { key: 'shipping_address' },
    ],
  },
  {
    ns: 'agent',
    fields: [
      { key: 'id' },
      { key: 'name' },
      { key: 'first_name' },
      { key: 'last_name' },
      { key: 'email' },
      { key: 'extension' },
      { key: 'role' },
    ],
  },
  {
    ns: 'tenant',
    fields: [
      { key: 'id' },
      { key: 'name' },
      { key: 'subdomain' },
      { key: 'timezone' },
      { key: 'plan_tier' },
    ],
  },
  {
    ns: 'input',
    dynamic: true,
    dynamicNote: 'Replace [node_id] with the ID of an Input node.',
    fields: [{ key: '[node_id]' }],
  },
  {
    ns: 'api',
    dynamic: true,
    dynamicNote: 'Replace [node_id] with the ID of an API Call node.',
    fields: [{ key: '[node_id].field' }],
  },
  {
    ns: 'flow',
    dynamic: true,
    dynamicNote: 'Replace [variable_name] with a name set via Set Variable.',
    fields: [{ key: '[variable_name]' }],
  },
]

interface Props {
  onInsert: (token: string) => void
}

export default function VariablePanel({ onInsert }: Props) {
  const [expanded, setExpanded] = useState<Set<string>>(
    new Set(['call_record', 'caller'])
  )

  function toggle(ns: string) {
    setExpanded((prev) => {
      const next = new Set(prev)
      next.has(ns) ? next.delete(ns) : next.add(ns)
      return next
    })
  }

  return (
    <div className="flex flex-col h-full">
      <div className="px-3 py-2.5 border-b border-gray-100 shrink-0">
        <p className="text-xs font-semibold text-gray-700">Available Variables</p>
        <p className="text-[10px] text-gray-400 mt-0.5">Click any field to insert at cursor</p>
      </div>

      <div className="flex-1 overflow-y-auto">
        {NAMESPACES.map((ns) => {
          const isOpen = expanded.has(ns.ns)
          return (
            <div key={ns.ns} className="border-b border-gray-100 last:border-0">
              {/* Namespace header */}
              <button
                type="button"
                onClick={() => toggle(ns.ns)}
                className="w-full flex items-center gap-1.5 px-3 py-2 hover:bg-gray-50 transition-colors text-left"
              >
                <svg
                  className={`w-3 h-3 text-gray-400 shrink-0 transition-transform duration-150 ${isOpen ? 'rotate-90' : ''}`}
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M9 5l7 7-7 7" />
                </svg>
                <span className="text-[11px] font-semibold text-gray-700 font-mono">{ns.ns}</span>
                {ns.dynamic && (
                  <span className="ml-auto text-[9px] bg-amber-100 text-amber-700 px-1.5 py-0.5 rounded font-medium shrink-0">
                    dynamic
                  </span>
                )}
              </button>

              {/* Fields */}
              {isOpen && (
                <div className="pb-1.5">
                  {ns.dynamicNote && (
                    <p className="px-7 pb-1 text-[10px] text-amber-600 italic leading-snug">
                      {ns.dynamicNote}
                    </p>
                  )}
                  {ns.fields.map((field) => {
                    const token = `{{${ns.ns}.${field.key}}}`
                    return (
                      <button
                        key={field.key}
                        type="button"
                        onClick={() => onInsert(token)}
                        title={`Insert ${token}`}
                        className="w-full text-left px-7 py-0.5 text-[11px] font-mono text-indigo-600 hover:bg-indigo-50 hover:text-indigo-900 transition-colors"
                      >
                        {field.key}
                      </button>
                    )
                  })}
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
