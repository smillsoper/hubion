import type { Node, Edge } from '@xyflow/react'
import type { NodeData } from '../types/designer'

export interface FlowVarToken {
  key: string
  label: string
  /** True when the variable holds a JSON object — show sub-properties, not the raw value. */
  isObject?: boolean
  properties?: { key: string; label: string }[]
}

export interface FlowAncestorVars {
  /** Input nodes that precede this node — each yields {{input.id}} */
  inputs: { id: string; label: string }[]
  /** API call nodes that precede this node — each yields {{api.id.field}} */
  apis: { id: string; label: string }[]
  /** Flow variables visible at this node — from set_variable, email, phone, and input outputVariable */
  flowVars: FlowVarToken[]
}

/** Return all ancestor node IDs via reverse-BFS from nodeId. */
function ancestorIds(nodeId: string, edges: Edge[]): Set<string> {
  const parents = new Map<string, string[]>()
  for (const e of edges) {
    const list = parents.get(e.target) ?? []
    list.push(e.source)
    parents.set(e.target, list)
  }

  const visited = new Set<string>()
  const queue: string[] = [nodeId]
  while (queue.length) {
    const cur = queue.shift()!
    if (visited.has(cur)) continue
    visited.add(cur)
    for (const p of parents.get(cur) ?? []) queue.push(p)
  }
  visited.delete(nodeId)
  return visited
}

/** Compute the variables that are reachable from ancestor nodes of nodeId. */
export function computeAncestorVars(
  nodeId: string,
  nodes: Node<NodeData>[],
  edges: Edge[],
): FlowAncestorVars {
  const ids = ancestorIds(nodeId, edges)
  const ancestors = nodes.filter((n) => ids.has(n.id))

  const inputs: FlowAncestorVars['inputs'] = []
  const apis: FlowAncestorVars['apis'] = []
  const flowVars: FlowVarToken[] = []
  const seen = new Set<string>()

  function addFlat(key: string, label: string) {
    if (!key || seen.has(key)) return
    seen.add(key)
    flowVars.push({ key, label })
  }

  function addObject(key: string, label: string, properties: { key: string; label: string }[]) {
    if (!key || seen.has(key)) return
    seen.add(key)
    flowVars.push({ key, label, isObject: true, properties })
  }

  for (const n of ancestors) {
    const type = n.type as string
    const data = n.data
    const nodeLabel = (data.label as string) || n.id

    switch (type) {
      case 'input': {
        inputs.push({ id: n.id, label: nodeLabel })
        const outVar = (data.outputVariable as string | undefined)?.trim()
        if (outVar) addFlat(outVar, `${nodeLabel} → output`)
        break
      }
      case 'email': {
        const outVar = (data.outputVariable as string | undefined)?.trim()
        if (outVar) {
          addObject(outVar, `Email: ${nodeLabel}`, [
            { key: 'value',         label: 'Email address' },
            { key: 'isFormatValid', label: 'Format valid' },
            { key: 'domainExists',  label: 'Domain exists' },
            { key: 'mxExists',      label: 'MX record exists' },
            { key: 'isDisposable',  label: 'Disposable address' },
            { key: 'isDeliverable', label: 'Deliverable' },
          ])
        }
        break
      }
      case 'phone': {
        const outVar = (data.outputVariable as string | undefined)?.trim()
        if (outVar) {
          addObject(outVar, `Phone: ${nodeLabel}`, [
            { key: 'value',         label: 'Digits (unmasked)' },
            { key: 'display_value', label: 'Formatted number' },
            { key: 'isMobile',      label: 'Is mobile' },
            { key: 'isTollFree',    label: 'Is toll-free' },
            { key: 'isInternal',    label: 'Is internal' },
            { key: 'doNotCall',     label: 'Do not call' },
          ])
        }
        break
      }
      case 'set_variable': {
        const assignments = data.assignments as { variable: string; value: string }[] | undefined
        for (const a of assignments ?? []) {
          let key = a.variable.trim()
          if (key.startsWith('{{') && key.endsWith('}}')) key = key.slice(2, -2).trim()
          if (key.startsWith('flow.')) addFlat(key.slice(5), `${nodeLabel} → set`)
          else if (key && !key.includes('.')) addFlat(key, `${nodeLabel} → set`)
        }
        break
      }
      case 'api_call': {
        apis.push({ id: n.id, label: nodeLabel })
        break
      }
    }
  }

  return { inputs, apis, flowVars }
}
