export type ContactConnectionNodeType =
  | 'script'
  | 'input'
  | 'branch'
  | 'set_variable'
  | 'api_call'
  | 'end'

export interface NodeData extends Record<string, unknown> {
  label: string
  isEntry?: boolean
  // script
  content?: string
  // input
  fieldType?: string
  required?: boolean
  options?: string
  // branch
  condition?: string
  // set_variable
  assignments?: { variable: string; value: string }[]
  // api_call
  method?: string
  url?: string
  headers?: string
  body?: string
  responseMap?: { source: string; target: string }[]
  // end
  status?: string
}

export interface ContactConnectionNodeDef {
  type: ContactConnectionNodeType
  label: string
  content?: string
  fieldType?: string
  required?: boolean
  options?: string
  condition?: string
  assignments?: { variable: string; value: string }[]
  method?: string
  url?: string
  headers?: string
  body?: string
  responseMap?: { source: string; target: string }[]
  status?: string
  _pos?: { x: number; y: number }
  transitions: Record<string, string>
}

export interface ContactConnectionFlowDefinition {
  flow_type: 'crm' | 'telephony'
  name: string
  entry_node: string
  nodes: Record<string, ContactConnectionNodeDef>
}

export const NODE_META: Record<
  ContactConnectionNodeType,
  { label: string; color: string; description: string; handles: 'single' | 'dual' | 'none' }
> = {
  script: {
    label: 'Script',
    color: '#3b82f6',
    description: 'Display text to the agent',
    handles: 'single',
  },
  input: {
    label: 'Input',
    color: '#10b981',
    description: 'Capture data from the agent',
    handles: 'single',
  },
  branch: {
    label: 'Branch',
    color: '#f59e0b',
    description: 'Conditional split on a variable',
    handles: 'dual',
  },
  set_variable: {
    label: 'Set Variable',
    color: '#8b5cf6',
    description: 'Assign a value to a flow variable',
    handles: 'single',
  },
  api_call: {
    label: 'API Call',
    color: '#6366f1',
    description: 'Call an external API endpoint',
    handles: 'dual',
  },
  end: {
    label: 'End',
    color: '#ef4444',
    description: 'Terminate the flow',
    handles: 'none',
  },
}

export function defaultNodeData(type: ContactConnectionNodeType): NodeData {
  switch (type) {
    case 'script':
      return { label: 'New Script', content: '' }
    case 'input':
      return { label: 'New Input', fieldType: 'text', required: false, options: '' }
    case 'branch':
      return { label: 'New Branch', condition: '' }
    case 'set_variable':
      return { label: 'Set Variable', assignments: [{ variable: '', value: '' }] }
    case 'api_call':
      return { label: 'New API Call', method: 'GET', url: '', headers: '', body: '' }
    case 'end':
      return { label: 'End', status: 'complete' }
  }
}
