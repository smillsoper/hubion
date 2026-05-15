export type ContactConnectionNodeType =
  | 'script'
  | 'input'
  | 'email'
  | 'phone'
  | 'branch'
  | 'set_variable'
  | 'api_call'
  | 'end'

export interface NodeData extends Record<string, unknown> {
  label: string
  isEntry?: boolean
  // script
  content?: string
  // input / email shared script
  scriptLabel?: string
  scriptContent?: string
  // input
  fieldType?: string
  required?: boolean
  options?: string
  outputVariable?: string
  minChars?: number
  maxChars?: number
  inputMask?: string
  customMask?: string
  // email
  checkARecord?: boolean
  checkMX?: boolean
  checkDisposable?: boolean
  // phone
  allowInternational?: boolean
  dncCheck?: boolean
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

export interface FlowOption { value: string; label: string }

export interface ContactConnectionNodeDef {
  type: ContactConnectionNodeType
  label: string
  content?: string
  scriptLabel?: string
  scriptContent?: string
  fieldType?: string
  required?: boolean
  options?: FlowOption[]
  outputVariable?: string
  minChars?: number
  maxChars?: number
  inputMask?: string
  customMask?: string
  checkARecord?: boolean
  checkMX?: boolean
  checkDisposable?: boolean
  allowInternational?: boolean
  dncCheck?: boolean
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
  _waypoints?: Record<string, { x: number; y: number }[]>
}

export const NODE_META: Record<
  ContactConnectionNodeType,
  { label: string; color: string; description: string; handles: 'single' | 'dual' | 'none' | 'custom' }
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
    handles: 'custom',
  },
  email: {
    label: 'Email',
    color: '#0891b2',
    description: 'Capture and validate an email address',
    handles: 'single',
  },
  phone: {
    label: 'Phone',
    color: '#0d9488',
    description: 'Capture and validate a phone number',
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
      return { label: 'New Input', scriptLabel: '', scriptContent: '', fieldType: 'text', required: false, options: '', outputVariable: '', minChars: undefined, maxChars: undefined, inputMask: '', customMask: '' }
    case 'email':
      return { label: 'Email', scriptLabel: '', scriptContent: '', outputVariable: '', required: false, checkARecord: false, checkMX: true, checkDisposable: true }
    case 'phone':
      return { label: 'Phone Number', scriptLabel: '', scriptContent: '', outputVariable: '', required: false, allowInternational: false, dncCheck: false }
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
