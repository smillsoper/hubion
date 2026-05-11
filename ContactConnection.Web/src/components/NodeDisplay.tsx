import { useState, type FormEvent } from 'react'
import type { FlowNodeState } from '../types/flow'

interface Props {
  node: FlowNodeState
  onAdvance: (input?: string) => void
  advancing: boolean
}

export default function NodeDisplay({ node, onAdvance, advancing }: Props) {
  const [inputValue, setInputValue] = useState('')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onAdvance(inputValue || undefined)
    setInputValue('')
  }

  function handleEmailSubmit(e: FormEvent) {
    e.preventDefault()
    // Always pass the value (even empty string) so the handler can distinguish
    // "submitted blank" from "first display" — required check is backend-enforced
    onAdvance(inputValue)
    setInputValue('')
  }

  if (node.nodeType === 'end') {
    return (
      <div className="flex flex-col items-center justify-center h-full gap-4 p-8 text-center">
        <div className="w-12 h-12 rounded-full bg-green-900/40 flex items-center justify-center">
          <svg className="w-6 h-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <p className="text-lg font-medium text-white">{node.label}</p>
        {node.content && (
          <p className="text-gray-400 text-sm max-w-md">{node.content}</p>
        )}
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6 p-6 max-w-2xl mx-auto">
      {/* Script context from preceding script node — shown above the current input/end node */}
      {node.scriptContext && (
        <div
          className="script-content bg-gray-900 rounded-xl p-5 text-gray-100 text-sm leading-relaxed border border-gray-800"
          dangerouslySetInnerHTML={{ __html: node.scriptContext }}
        />
      )}

      {/* Node's own content (e.g. input prompt) */}
      {node.content && (
        <div
          className="script-content bg-gray-900 rounded-xl p-5 text-gray-100 text-sm leading-relaxed border border-gray-800"
          dangerouslySetInnerHTML={{ __html: node.content }}
        />
      )}

      {/* Inline script (embedded on input/email nodes) */}
      {node.nodeScriptLabel && (
        <p className="text-xs font-semibold text-gray-300 uppercase tracking-wider">
          {node.nodeScriptLabel}
        </p>
      )}
      {node.nodeScriptContent && (
        <div
          className="script-content bg-gray-900 rounded-xl p-5 text-gray-100 text-sm leading-relaxed border border-gray-800"
          dangerouslySetInnerHTML={{ __html: node.nodeScriptContent }}
        />
      )}

      {/* Step label */}
      <div className="flex items-center gap-2">
        <span className="text-xs uppercase tracking-wider text-gray-500 font-medium">
          {node.nodeType.replace('_', ' ')}
        </span>
        <span className="text-white font-medium">{node.label}</span>
      </div>

      {/* Input area */}
      {node.nodeType === 'input' && (
        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          {node.inputType === 'select' && node.options ? (
            <select
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500 border border-gray-700"
            >
              <option value="">Select…</option>
              {node.options.map((o) => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          ) : node.inputType === 'checkbox' ? (
            <label className="flex items-center gap-2 text-sm text-gray-200 cursor-pointer">
              <input
                type="checkbox"
                checked={inputValue === 'true'}
                onChange={(e) => setInputValue(e.target.checked ? 'true' : 'false')}
                className="rounded accent-indigo-500"
              />
              {node.label}
            </label>
          ) : node.inputType === 'date' ? (
            <input
              type="date"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500 border border-gray-700"
            />
          ) : (
            <input
              type={node.inputType === 'phone' ? 'tel' : 'text'}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Enter value…"
              className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500 border border-gray-700"
            />
          )}

          <button
            type="submit"
            disabled={advancing}
            className="self-start bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white rounded-lg px-5 py-2 text-sm font-medium transition-colors"
          >
            {advancing ? 'Advancing…' : 'Next'}
          </button>
        </form>
      )}

      {/* Email node */}
      {node.nodeType === 'email' && (
        <form onSubmit={handleEmailSubmit} className="flex flex-col gap-3">
          {node.validationError && (
            <p className="text-sm text-red-400 bg-red-950/40 border border-red-800 rounded-lg px-3 py-2">
              {node.validationError}
            </p>
          )}
          <input
            type="email"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Enter email address…"
            required={node.required}
            className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-cyan-500 border border-gray-700"
          />
          <button
            type="submit"
            disabled={advancing}
            className="self-start bg-cyan-700 hover:bg-cyan-600 disabled:opacity-50 text-white rounded-lg px-5 py-2 text-sm font-medium transition-colors"
          >
            {advancing ? 'Validating…' : 'Next'}
          </button>
        </form>
      )}

      {/* Script node — just a Next button */}
      {(node.nodeType === 'script' || node.nodeType === 'branch' || node.nodeType === 'set_variable' || node.nodeType === 'api_call') && (
        <button
          onClick={() => onAdvance()}
          disabled={advancing}
          className="self-start bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white rounded-lg px-5 py-2 text-sm font-medium transition-colors"
        >
          {advancing ? 'Advancing…' : 'Continue'}
        </button>
      )}
    </div>
  )
}
