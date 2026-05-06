import { useCallback, useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import type { EdgeProps } from '@xyflow/react'
import { EdgeLabelRenderer, useReactFlow } from '@xyflow/react'

interface Pt { x: number; y: number }

function buildPath(pts: Pt[]): string {
  if (pts.length < 2) return ''
  if (pts.length === 2) {
    const dy = Math.abs(pts[1].y - pts[0].y)
    const off = Math.max(40, dy * 0.4)
    return (
      `M ${pts[0].x} ${pts[0].y} ` +
      `C ${pts[0].x} ${pts[0].y + off}, ` +
      `${pts[1].x} ${pts[1].y - off}, ` +
      `${pts[1].x} ${pts[1].y}`
    )
  }
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 0; i < pts.length - 1; i++) {
    const p0 = pts[Math.max(0, i - 1)]
    const p1 = pts[i]
    const p2 = pts[i + 1]
    const p3 = pts[Math.min(pts.length - 1, i + 2)]
    const cp1x = p1.x + (p2.x - p0.x) / 6
    const cp1y = p1.y + (p2.y - p0.y) / 6
    const cp2x = p2.x - (p3.x - p1.x) / 6
    const cp2y = p2.y - (p3.y - p1.y) / 6
    d += ` C ${cp1x} ${cp1y}, ${cp2x} ${cp2y}, ${p2.x} ${p2.y}`
  }
  return d
}

function distToSegment(p: Pt, a: Pt, b: Pt): number {
  const dx = b.x - a.x, dy = b.y - a.y
  const lenSq = dx * dx + dy * dy
  if (lenSq === 0) return Math.hypot(p.x - a.x, p.y - a.y)
  const t = Math.max(0, Math.min(1, ((p.x - a.x) * dx + (p.y - a.y) * dy) / lenSq))
  return Math.hypot(p.x - a.x - t * dx, p.y - a.y - t * dy)
}

function nearestSegmentIndex(click: Pt, allPts: Pt[]): number {
  let best = 0, bestDist = Infinity
  for (let i = 0; i < allPts.length - 1; i++) {
    const d = distToSegment(click, allPts[i], allPts[i + 1])
    if (d < bestDist) { bestDist = d; best = i }
  }
  return best
}

interface ContextMenu { waypointIdx: number; x: number; y: number }

export default function EditableEdge({
  id, sourceX, sourceY, targetX, targetY, data, selected, markerEnd,
}: EdgeProps) {
  const { setEdges, screenToFlowPosition } = useReactFlow()
  const waypoints = (data?.waypoints ?? []) as Pt[]
  const [menu, setMenu] = useState<ContextMenu | null>(null)
  const draggingIdx = useRef<number | null>(null)
  const dragSnapshot = useRef<Pt[]>([])

  const allPts: Pt[] = [{ x: sourceX, y: sourceY }, ...waypoints, { x: targetX, y: targetY }]
  const pathD = buildPath(allPts)
  const stroke = selected ? '#6366f1' : '#9ca3af'

  const setWaypoints = useCallback(
    (wps: Pt[]) =>
      setEdges(eds =>
        eds.map(e => e.id === id ? { ...e, data: { ...e.data, waypoints: wps } } : e),
      ),
    [id, setEdges],
  )

  useEffect(() => {
    if (!menu) return
    const close = () => setMenu(null)
    document.addEventListener('click', close, { capture: true })
    return () => document.removeEventListener('click', close, { capture: true })
  }, [menu])

  const handlePathClick = (e: React.MouseEvent<SVGPathElement>) => {
    e.stopPropagation()
    const pos = screenToFlowPosition({ x: e.clientX, y: e.clientY })
    const segIdx = nearestSegmentIndex(pos, allPts)
    const next = [...waypoints]
    next.splice(segIdx, 0, pos)
    setWaypoints(next)
  }

  return (
    <>
      {/* Visible path */}
      <path
        id={id}
        d={pathD}
        fill="none"
        stroke={stroke}
        strokeWidth={selected ? 2.5 : 1.5}
        markerEnd={markerEnd}
        className="react-flow__edge-path"
      />
      {/* Wide invisible hit area — click anywhere on the line to add a waypoint */}
      <path
        d={pathD}
        fill="none"
        stroke="transparent"
        strokeWidth={16}
        onClick={handlePathClick}
        style={{ cursor: 'crosshair' }}
      />

      {/*
        Waypoint handles rendered as HTML divs via EdgeLabelRenderer.
        This places them in a separate overlay OUTSIDE React Flow's SVG
        connection layer, so React Flow's crosshair / connection handler
        never intercepts pointer events on them.
        "nodrag nopan" classes tell React Flow to ignore these elements.
      */}
      <EdgeLabelRenderer>
        {waypoints.map((wp, i) => (
          <div
            key={i}
            className="nodrag nopan"
            style={{
              position: 'absolute',
              transform: `translate(-50%, -50%) translate(${wp.x}px, ${wp.y}px)`,
              width: 14,
              height: 14,
              borderRadius: '50%',
              background: selected ? '#6366f1' : '#6b7280',
              border: '2px solid #fff',
              boxShadow: '0 1px 4px rgba(0,0,0,0.4)',
              cursor: 'grab',
              pointerEvents: 'all',
              touchAction: 'none',
            }}
            onPointerDown={e => {
              e.stopPropagation()
              e.preventDefault()
              e.currentTarget.setPointerCapture(e.pointerId)
              draggingIdx.current = i
              dragSnapshot.current = [...waypoints]
            }}
            onPointerMove={e => {
              if (draggingIdx.current !== i) return
              e.stopPropagation()
              const pos = screenToFlowPosition({ x: e.clientX, y: e.clientY })
              setWaypoints(dragSnapshot.current.map((wp, j) => j === i ? pos : wp))
            }}
            onPointerUp={e => {
              if (draggingIdx.current !== i) return
              e.currentTarget.releasePointerCapture(e.pointerId)
              draggingIdx.current = null
            }}
            onContextMenu={e => {
              e.preventDefault()
              e.stopPropagation()
              setMenu({ waypointIdx: i, x: e.clientX, y: e.clientY })
            }}
          />
        ))}
      </EdgeLabelRenderer>

      {menu &&
        createPortal(
          <div
            style={{
              position: 'fixed',
              left: menu.x,
              top: menu.y,
              background: '#1f2937',
              border: '1px solid #374151',
              borderRadius: 8,
              boxShadow: '0 8px 24px rgba(0,0,0,0.5)',
              zIndex: 9999,
              minWidth: 170,
              overflow: 'hidden',
            }}
            onClick={e => e.stopPropagation()}
          >
            <button
              style={{
                display: 'block', width: '100%',
                padding: '9px 16px', textAlign: 'left',
                color: '#f87171', background: 'none', border: 'none',
                cursor: 'pointer', fontSize: 13, fontFamily: 'inherit',
              }}
              onClick={() => {
                setWaypoints(waypoints.filter((_, j) => j !== menu.waypointIdx))
                setMenu(null)
              }}
            >
              Delete curve point
            </button>
          </div>,
          document.body,
        )}
    </>
  )
}
