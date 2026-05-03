import { api } from './client'
import type { FlowNodeState, StartSessionRequest, AdvanceSessionRequest } from '../types/flow'
import type { ContactConnectionFlowDefinition } from '../types/designer'

export interface FlowSummary {
  id: string
  name: string
  flow_type: string
  is_active: boolean
  version: number
}

export interface FlowDetail extends FlowSummary {
  definition: string
}

export const flowsApi = {
  // Agent panel
  list: () => api.get<FlowSummary[]>('/api/v1/flows'),

  startSession: (req: StartSessionRequest) =>
    api.post<FlowNodeState>('/api/v1/flow-sessions', req),

  getSession: (sessionId: string) =>
    api.get<FlowNodeState>(`/api/v1/flow-sessions/${sessionId}`),

  advance: (sessionId: string, req: AdvanceSessionRequest) =>
    api.post<FlowNodeState>(`/api/v1/flow-sessions/${sessionId}/advance`, req),

  // Flow designer
  create: (name: string, flowType: string, definition: ContactConnectionFlowDefinition) =>
    api.post<FlowDetail>('/api/v1/flows', {
      name,
      flowType,
      definition: JSON.stringify(definition),
    }),

  getDetail: (id: string) => api.get<FlowDetail>(`/api/v1/flows/${id}`),

  updateDefinition: (id: string, definition: ContactConnectionFlowDefinition) =>
    api.put<FlowDetail>(`/api/v1/flows/${id}`, { definition: JSON.stringify(definition) }),

  publish: (id: string) => api.post<FlowDetail>(`/api/v1/flows/${id}/publish`),
}
