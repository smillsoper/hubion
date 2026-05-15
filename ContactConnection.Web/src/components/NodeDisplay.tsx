import { useState, useEffect, useRef, type FormEvent } from 'react'
import type { FlowNodeState } from '../types/flow'

// ── WinForms-style input mask engine ──────────────────────────────────────
// Mask chars: 0=digit  9=digit/space  L=letter  ?=letter/space  A=alphanum  &=any required  C=any optional
// All other characters are literals and are auto-inserted.

const MASK_CHARS = new Set(['0', '9', 'L', '?', 'A', 'a', '&', 'C'])
const MASK_REQUIRED = new Set(['0', 'L', 'A', '&'])

function maskCharValid(ch: string, mc: string): boolean {
  switch (mc) {
    case '0': return /\d/.test(ch)
    case '9': return /[\d ]/.test(ch)
    case 'L': return /[a-zA-Z]/.test(ch)
    case '?': return /[a-zA-Z ]/.test(ch)
    case 'A': return /[a-zA-Z0-9]/.test(ch)
    case 'a': return /[a-zA-Z0-9 ]/.test(ch)
    case '&': return ch.length === 1
    case 'C': return true
    default:  return false
  }
}

// Format a user input string against a mask, auto-inserting literal characters.
// Strips any existing literals from userInput first so re-formatting is idempotent.
function applyMask(userInput: string, mask: string): string {
  // Keep only alphanumeric chars (covers 0/9/L/A patterns); '&'/'C' users may need special chars but that's uncommon
  const raw = [...userInput].filter(c => /[a-zA-Z0-9]/.test(c))
  let result = ''
  let ri = 0

  for (let mi = 0; mi < mask.length; mi++) {
    if (ri >= raw.length) break
    const mc = mask[mi]
    if (!MASK_CHARS.has(mc)) {
      result += mc   // literal — auto-insert, don't consume raw
      continue
    }
    // Skip raw chars that don't satisfy this mask position
    while (ri < raw.length && !maskCharValid(raw[ri], mc)) ri++
    if (ri < raw.length) {
      result += raw[ri]
      ri++
    }
  }
  return result
}

// True when all required mask positions are filled
function isMaskComplete(value: string, mask: string): boolean {
  // The fully-filled mask has length === mask.length
  // Since applyMask inserts all literals exactly once, complete fill = full length
  return value.length === mask.length
    && [...mask].every((mc, i) => !MASK_REQUIRED.has(mc) || (i < value.length && maskCharValid(value[i] ?? '', mc)))
}

interface Props {
  node: FlowNodeState
  onAdvance: (input?: string) => void
  advancing: boolean
}

export default function NodeDisplay({ node, onAdvance, advancing }: Props) {
  const [inputValue, setInputValue] = useState('')
  const [localError, setLocalError] = useState<string | null>(null)

  // Auto-focus the primary input whenever the displayed node changes
  const focusRef = useRef<HTMLElement | null>(null)
  useEffect(() => {
    setLocalError(null)
    focusRef.current?.focus()
  }, [node])

  function handleTextChange(raw: string) {
    const mask = node.inputMask
    setLocalError(null)
    setInputValue(mask ? applyMask(raw, mask) : raw)
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const mask = node.inputMask
    if (mask) {
      const isEmpty   = inputValue.length === 0
      const complete  = isMaskComplete(inputValue, mask)
      if (!isEmpty && !complete) {
        // Partial entry is never permitted — required or not
        setLocalError(
          node.required
            ? 'Please complete the entire field before continuing.'
            : 'Please complete the entire field or clear it before continuing.',
        )
        return
      }
      if (node.required && isEmpty) {
        setLocalError('This field is required.')
        return
      }
    } else {
      const len = inputValue.length
      if (node.minChars && len < node.minChars) {
        setLocalError(`Minimum ${node.minChars} characters required (${len} entered).`)
        return
      }
      if (node.maxChars && len > node.maxChars) {
        setLocalError(`Maximum ${node.maxChars} characters allowed (${len} entered).`)
        return
      }
    }
    onAdvance(inputValue)
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
              ref={(el) => { focusRef.current = el }}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm input-focus-glow border border-gray-700"
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
          ) : (
            /* text (default) — supports mask + min/max */
            <div className="flex flex-col gap-1">
              <input
                ref={(el) => { focusRef.current = el }}
                type="text"
                value={inputValue}
                onChange={(e) => handleTextChange(e.target.value)}
                placeholder={node.inputMask ? node.inputMask.replace(/0/g, '_').replace(/[LA]/g, '_') : 'Enter value…'}
                className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm input-focus-glow border border-gray-700 font-mono"
              />
              {(node.minChars || node.maxChars) && !node.inputMask && (
                <p className="text-[11px] text-gray-500">
                  {node.minChars && node.maxChars
                    ? `${node.minChars}–${node.maxChars} characters`
                    : node.minChars
                    ? `Minimum ${node.minChars} characters`
                    : `Maximum ${node.maxChars} characters`}
                  {inputValue.length > 0 && ` · ${inputValue.length} entered`}
                </p>
              )}
            </div>
          )}

          {localError && (
            <p className="text-sm text-red-400 bg-red-950/40 border border-red-800 rounded-lg px-3 py-2">
              {localError}
            </p>
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

      {/* Phone node */}
      {node.nodeType === 'phone' && (
        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          {node.validationError && (
            <p className="text-sm text-red-400 bg-red-950/40 border border-red-800 rounded-lg px-3 py-2">
              {node.validationError}
            </p>
          )}
          <input
            ref={(el) => { focusRef.current = el }}
            type="text"
            value={inputValue}
            onChange={(e) => handleTextChange(e.target.value)}
            placeholder={node.inputMask ?? 'Enter phone number…'}
            className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm input-focus-glow-teal border border-gray-700 font-mono"
          />
          {localError && (
            <p className="text-sm text-red-400 bg-red-950/40 border border-red-800 rounded-lg px-3 py-2">
              {localError}
            </p>
          )}
          <button
            type="submit"
            disabled={advancing}
            className="self-start bg-teal-700 hover:bg-teal-600 disabled:opacity-50 text-white rounded-lg px-5 py-2 text-sm font-medium transition-colors"
          >
            {advancing ? 'Validating…' : 'Next'}
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
            ref={(el) => { focusRef.current = el }}
            type="email"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Enter email address…"
            required={node.required}
            className="bg-gray-800 text-white rounded-lg px-3 py-2 text-sm input-focus-glow-cyan border border-gray-700"
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
