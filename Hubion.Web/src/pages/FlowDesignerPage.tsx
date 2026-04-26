import { useCallback, useEffect, useRef, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  ReactFlow,
  ReactFlowProvider,
  useNodesState,
  useEdgesState,
  addEdge,
  Background,
  Controls,
  MiniMap,
  useReactFlow,
  type Node,
  type Edge,
  type Connection,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import { flowsApi } from '../api/flows'
import NodePalette from '../components/designer/NodePalette'
import NodePropertiesPanel from '../components/designer/NodePropertiesPanel'
import ScriptNode from '../components/designer/nodes/ScriptNode'
import InputNode from '../components/designer/nodes/InputNode'
import BranchNode from '../components/designer/nodes/BranchNode'
import SetVariableNode from '../components/designer/nodes/SetVariableNode'
import ApiCallNode from '../components/designer/nodes/ApiCallNode'
import EndNode from '../components/designer/nodes/EndNode'

import type { NodeData, HubionNodeType, HubionFlowDefinition } from '../types/designer'
import { defaultNodeData } from '../types/designer'

const nodeTypes = {
  script: ScriptNode,
  input: InputNode,
  branch: BranchNode,
  set_variable: SetVariableNode,
  api_call: ApiCallNode,
  end: EndNode,
}

// ── Conversion helpers ──────────────────────────────────────────────────────

function toHubionDef(
  nodes: Node<NodeData>[],
  edges: Edge[],
  entryNodeId: string | null,
  flowName: string,
): HubionFlowDefinition {
  const hubionNodes: HubionFlowDefinition['nodes'] = {}
  for (const n of nodes) {
    const outgoing = edges.filter((e) => e.source === n.id)
    const transitions: Record<string, string> = {}
    for (const e of outgoing) {
      transitions[e.sourceHandle ?? 'default'] = e.target
    }
    const { isEntry: _entry, ...rest } = n.data
    hubionNodes[n.id] = {
      ...rest,
      type: (n.type ?? 'script') as HubionNodeType,
      label: n.data.label as string,
      _pos: n.position,
      transitions,
    }
  }
  return {
    flow_type: 'crm',
    name: flowName,
    entry_node: entryNodeId ?? nodes[0]?.id ?? '',
    nodes: hubionNodes,
  }
}

function fromHubionDef(def: HubionFlowDefinition): {
  nodes: Node<NodeData>[]
  edges: Edge[]
  entryNodeId: string
} {
  const nodes: Node<NodeData>[] = []
  const edges: Edge[] = []
  let x = 100

  for (const [id, nodeDef] of Object.entries(def.nodes)) {
    const { type, label, _pos, transitions, ...rest } = nodeDef
    nodes.push({
      id,
      type,
      position: _pos ?? { x, y: 100 },
      data: { label, isEntry: id === def.entry_node, ...rest },
    })
    x += 260

    for (const [handle, targetId] of Object.entries(transitions)) {
      edges.push({
        id: `${id}-${handle}-${targetId}`,
        source: id,
        target: targetId,
        sourceHandle: handle === 'default' ? null : handle,
        type: 'smoothstep',
      })
    }
  }

  return { nodes, edges, entryNodeId: def.entry_node }
}

// ── Inner canvas (needs useReactFlow) ──────────────────────────────────────

function DesignerCanvas({
  initialFlowId,
  initialFlowName,
}: {
  initialFlowId: string | null
  initialFlowName: string
}) {
  const navigate = useNavigate()
  const { screenToFlowPosition } = useReactFlow()
  const wrapperRef = useRef<HTMLDivElement>(null)

  const [nodes, setNodes, onNodesChange] = useNodesState<Node<NodeData>>([])
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([])
  const [flowName, setFlowName] = useState(initialFlowName)
  const [flowId, setFlowId] = useState<string | null>(initialFlowId)
  const [entryNodeId, setEntryNodeId] = useState<string | null>(null)
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)
  const [publishing, setPublishing] = useState(false)
  const [statusMsg, setStatusMsg] = useState('')

  // Load existing flow
  useEffect(() => {
    if (!initialFlowId) return
    flowsApi.getDetail(initialFlowId).then((detail) => {
      setFlowName(detail.name)
      try {
        const def: HubionFlowDefinition = JSON.parse(detail.definition)
        const { nodes: n, edges: e, entryNodeId: entry } = fromHubionDef(def)
        setNodes(n)
        setEdges(e)
        setEntryNodeId(entry)
      } catch {
        // definition not yet set — start with empty canvas
      }
    })
  }, [initialFlowId, setNodes, setEdges])

  // Connection
  const onConnect = useCallback(
    (params: Connection) =>
      setEdges((eds) => addEdge({ ...params, type: 'smoothstep' }, eds)),
    [setEdges],
  )

  // Node click → open properties
  const onNodeClick = useCallback((_e: React.MouseEvent, node: Node) => {
    setSelectedNodeId(node.id)
  }, [])

  // Deselect on background click
  const onPaneClick = useCallback(() => {
    setSelectedNodeId(null)
  }, [])

  // Drag-and-drop from palette
  const onDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.dataTransfer.dropEffect = 'move'
  }, [])

  const onDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault()
      const type = e.dataTransfer.getData('application/reactflow-node-type') as HubionNodeType
      if (!type) return

      const position = screenToFlowPosition({ x: e.clientX, y: e.clientY })
      const id = `node_${Date.now()}`
      const isFirst = nodes.length === 0

      const newNode: Node<NodeData> = {
        id,
        type,
        position,
        data: { ...defaultNodeData(type), isEntry: isFirst },
      }

      setNodes((nds) => [...nds, newNode])
      if (isFirst) setEntryNodeId(id)
      setSelectedNodeId(id)
    },
    [screenToFlowPosition, setNodes, nodes.length],
  )

  // Update node data from properties panel
  const updateNodeData = useCallback(
    (id: string, patch: Partial<NodeData>) => {
      setNodes((nds) =>
        nds.map((n) => (n.id === id ? { ...n, data: { ...n.data, ...patch } } : n)),
      )
    },
    [setNodes],
  )

  // Set entry node
  const setEntryNode = useCallback(
    (id: string) => {
      setEntryNodeId(id)
      setNodes((nds) =>
        nds.map((n) => ({ ...n, data: { ...n.data, isEntry: n.id === id } })),
      )
    },
    [setNodes],
  )

  // Delete node + connected edges
  const deleteNode = useCallback(
    (id: string) => {
      setNodes((nds) => nds.filter((n) => n.id !== id))
      setEdges((eds) => eds.filter((e) => e.source !== id && e.target !== id))
      setSelectedNodeId(null)
      if (entryNodeId === id) setEntryNodeId(null)
    },
    [setNodes, setEdges, entryNodeId],
  )

  // Save
  const onSave = async () => {
    setSaving(true)
    setStatusMsg('')
    try {
      const def = toHubionDef(nodes as Node<NodeData>[], edges, entryNodeId, flowName)
      if (flowId) {
        await flowsApi.updateDefinition(flowId, def)
        setStatusMsg('Saved')
      } else {
        const detail = await flowsApi.create(flowName, 'crm', def)
        setFlowId(detail.id)
        navigate(`/designer/${detail.id}`, { replace: true })
        setStatusMsg('Saved')
      }
    } catch (err) {
      setStatusMsg(`Error: ${String(err)}`)
    } finally {
      setSaving(false)
      setTimeout(() => setStatusMsg(''), 3000)
    }
  }

  // Publish
  const onPublish = async () => {
    if (!flowId) {
      await onSave()
    }
    if (!flowId) return
    setPublishing(true)
    setStatusMsg('')
    try {
      await flowsApi.publish(flowId)
      setStatusMsg('Published — flow is now active')
    } catch (err) {
      setStatusMsg(`Error: ${String(err)}`)
    } finally {
      setPublishing(false)
      setTimeout(() => setStatusMsg(''), 4000)
    }
  }

  const selectedNode = selectedNodeId ? (nodes as Node<NodeData>[]).find((n) => n.id === selectedNodeId) : null

  return (
    <div className="flex flex-col" style={{ height: '100vh' }}>
      {/* Top bar */}
      <div className="flex items-center gap-3 px-4 py-2 bg-white border-b border-gray-200 shrink-0">
        <img src="/hubion-logo.svg" alt="Hubion" className="h-8 shrink-0" />
        <div className="w-px h-5 bg-gray-200 shrink-0" />
        <button
          className="text-gray-500 hover:text-gray-700 text-sm flex items-center gap-1"
          onClick={() => navigate('/agent')}
        >
          ← Back
        </button>
        <input
          className="flex-1 max-w-xs border border-gray-300 rounded px-3 py-1 text-sm font-medium focus:outline-none focus:border-blue-400"
          value={flowName}
          onChange={(e) => setFlowName(e.target.value)}
          placeholder="Flow name"
        />
        {statusMsg && (
          <span className="text-sm text-gray-500 italic">{statusMsg}</span>
        )}
        <div className="ml-auto flex items-center gap-2">
          <button
            className="px-4 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 transition-colors"
            onClick={onSave}
            disabled={saving}
          >
            {saving ? 'Saving…' : 'Save'}
          </button>
          <button
            className="px-4 py-1.5 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 transition-colors"
            onClick={onPublish}
            disabled={publishing}
          >
            {publishing ? 'Publishing…' : 'Publish'}
          </button>
        </div>
      </div>

      {/* 3-panel layout */}
      <div className="flex flex-1 overflow-hidden">
        <NodePalette />

        {/* Canvas */}
        <div ref={wrapperRef} className="flex-1">
          <ReactFlow
            nodes={nodes}
            edges={edges}
            nodeTypes={nodeTypes}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onNodeClick={onNodeClick}
            onPaneClick={onPaneClick}
            onDragOver={onDragOver}
            onDrop={onDrop}
            fitView
            deleteKeyCode="Delete"
          >
            <Background />
            <Controls />
            <MiniMap nodeColor={(n) => {
              const meta: Record<string, string> = {
                script: '#3b82f6',
                input: '#10b981',
                branch: '#f59e0b',
                set_variable: '#8b5cf6',
                api_call: '#6366f1',
                end: '#ef4444',
              }
              return meta[n.type ?? ''] ?? '#9ca3af'
            }} />
          </ReactFlow>
        </div>

        {/* Properties panel */}
        {selectedNode && (
          <NodePropertiesPanel
            node={selectedNode}
            isEntry={entryNodeId === selectedNode.id}
            onUpdate={updateNodeData}
            onSetEntry={setEntryNode}
            onDelete={deleteNode}
            onClose={() => setSelectedNodeId(null)}
          />
        )}
      </div>
    </div>
  )
}

// ── Page wrapper (provides ReactFlow context) ───────────────────────────────

export default function FlowDesignerPage() {
  const { id } = useParams<{ id?: string }>()
  return (
    <ReactFlowProvider>
      <DesignerCanvas initialFlowId={id ?? null} initialFlowName="New Flow" />
    </ReactFlowProvider>
  )
}
