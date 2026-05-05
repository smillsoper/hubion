import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { flowsApi, type FlowSummary } from '../api/flows'

export default function FlowsPage() {
  const navigate = useNavigate()
  const [flows, setFlows] = useState<FlowSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [publishingId, setPublishingId] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const data = await flowsApi.listAll()
      setFlows(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load flows')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handlePublish(id: string) {
    setPublishingId(id)
    try {
      await flowsApi.publish(id)
      await load()
    } finally {
      setPublishingId(null)
    }
  }

  async function handleDelete(id: string, name: string) {
    if (!window.confirm(`Delete "${name}"? This cannot be undone.`)) return
    setDeletingId(id)
    try {
      await flowsApi.delete(id)
      await load()
    } finally {
      setDeletingId(null)
    }
  }

  function fmt(iso: string) {
    return new Date(iso).toLocaleDateString(undefined, {
      month: 'short', day: 'numeric', year: 'numeric',
    })
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-3 bg-white border-b border-gray-200 shrink-0">
        <div className="flex items-center gap-3">
          <img src="/cc-navbar-light.svg" alt="Contact Connection" className="h-8 shrink-0" />
          <div className="w-px h-5 bg-gray-200" />
          <button
            onClick={() => navigate('/agent')}
            className="text-gray-500 hover:text-gray-700 text-sm flex items-center gap-1"
          >
            ← Back
          </button>
          <span className="text-sm font-semibold text-gray-800">Flows</span>
        </div>
        <button
          onClick={() => navigate('/designer')}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-1.5 rounded-lg transition-colors"
        >
          + New Flow
        </button>
      </div>

      {/* Content */}
      <div className="flex-1 p-6">
        {loading ? (
          <div className="flex items-center justify-center h-40 text-gray-400 text-sm">
            Loading…
          </div>
        ) : error ? (
          <div className="flex flex-col items-center justify-center h-40 gap-3">
            <p className="text-red-500 text-sm font-medium">Error loading flows</p>
            <p className="text-red-400 text-xs font-mono">{error}</p>
            <button onClick={load} className="text-sm text-blue-600 hover:underline">Retry</button>
          </div>
        ) : flows.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-40 gap-3">
            <p className="text-gray-400 text-sm">No flows yet.</p>
            <button
              onClick={() => navigate('/designer')}
              className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-1.5 rounded-lg transition-colors"
            >
              Create your first flow
            </button>
          </div>
        ) : (
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50">
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Name</th>
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Type</th>
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Version</th>
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Updated</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody>
                {flows.map((flow) => (
                  <tr key={flow.id} className="border-b border-gray-100 last:border-0 hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-800">{flow.name}</td>
                    <td className="px-4 py-3 text-gray-500 capitalize">{flow.flow_type}</td>
                    <td className="px-4 py-3">
                      {flow.is_active ? (
                        <span className="inline-flex items-center gap-1 text-xs font-medium text-emerald-700 bg-emerald-50 border border-emerald-200 px-2 py-0.5 rounded-full">
                          <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
                          Published
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 text-xs font-medium text-amber-700 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded-full">
                          <span className="w-1.5 h-1.5 rounded-full bg-amber-400" />
                          Draft
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-500">v{flow.version}</td>
                    <td className="px-4 py-3 text-gray-400">{fmt(flow.updated_at)}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        {!flow.is_active && (
                          <button
                            onClick={() => handlePublish(flow.id)}
                            disabled={publishingId === flow.id}
                            className="text-xs text-blue-600 hover:text-blue-700 border border-blue-200 hover:border-blue-300 rounded px-2.5 py-1 disabled:opacity-50 transition-colors"
                          >
                            {publishingId === flow.id ? 'Publishing…' : 'Publish'}
                          </button>
                        )}
                        <button
                          onClick={() => navigate(`/designer/${flow.id}`)}
                          className="text-xs text-gray-600 hover:text-gray-800 border border-gray-200 hover:border-gray-300 rounded px-2.5 py-1 transition-colors"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(flow.id, flow.name)}
                          disabled={deletingId === flow.id}
                          className="text-xs text-red-500 hover:text-red-700 border border-red-200 hover:border-red-300 rounded px-2.5 py-1 disabled:opacity-50 transition-colors"
                        >
                          {deletingId === flow.id ? 'Deleting…' : 'Delete'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
