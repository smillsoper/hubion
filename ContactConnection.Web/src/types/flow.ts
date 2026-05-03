export interface FlowOption {
  value: string
  label: string
}

export interface FlowNodeState {
  sessionId: string
  nodeId: string
  nodeType: 'script' | 'input' | 'branch' | 'set_variable' | 'api_call' | 'end'
  label: string
  content?: string
  inputType?: 'text' | 'select' | 'checkbox' | 'date' | 'address' | 'phone'
  options?: FlowOption[]
  condition?: string
  isTerminal: boolean
  lockedFields: string[]
}

export interface StartSessionRequest {
  flowId: string
  callRecordId?: string
}

export interface AdvanceSessionRequest {
  inputValue?: string
  transition?: string
}
