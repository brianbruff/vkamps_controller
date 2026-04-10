import type { AmpState } from '../App'
import { useRef, useEffect, useCallback } from 'react'
import './Dashboard.css'

interface Props {
  state: AmpState
  onAction: (action: string) => void
}

/* ── Spectrum Canvas ─────────────────────────────────────────── */
function SpectrumCanvas({ power }: { power: number }) {
  const ref = useRef<HTMLCanvasElement>(null)

  const draw = useCallback(() => {
    const canvas = ref.current
    if (!canvas) return
    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const w = canvas.width
    const h = canvas.height
    ctx.clearRect(0, 0, w, h)

    const barCount = 40
    const barW = w / barCount - 1
    const intensity = Math.min(power / 1500, 1)

    for (let i = 0; i < barCount; i++) {
      const t = i / barCount
      const barH = (0.3 + 0.7 * Math.pow(t, 0.5) * intensity) * h * (0.6 + 0.4 * Math.random())

      // Color gradient: green → yellow → orange → red
      let r: number, g: number, b: number
      if (t < 0.5) {
        r = Math.floor(t * 2 * 255)
        g = 255
        b = 0
      } else if (t < 0.85) {
        r = 255
        g = Math.floor((1 - (t - 0.5) / 0.35) * 255)
        b = 0
      } else {
        r = 255
        g = 0
        b = 0
      }

      const x = i * (barW + 1)
      const gradient = ctx.createLinearGradient(x, h, x, h - barH)
      gradient.addColorStop(0, `rgba(${r},${g},${b},0.9)`)
      gradient.addColorStop(0.5, `rgba(${r},${g},${b},0.6)`)
      gradient.addColorStop(1, `rgba(${r},${g},${b},0.2)`)

      ctx.fillStyle = gradient
      ctx.fillRect(x, h - barH, barW, barH)

      // Glow effect
      ctx.shadowColor = `rgb(${r},${g},${b})`
      ctx.shadowBlur = 4
      ctx.fillStyle = `rgba(${r},${g},${b},0.4)`
      ctx.fillRect(x, h - barH, barW, 2)
      ctx.shadowBlur = 0
    }
  }, [power])

  useEffect(() => {
    const id = requestAnimationFrame(draw)
    return () => cancelAnimationFrame(id)
  }, [draw])

  return <canvas ref={ref} width={300} height={150} className="spectrum-canvas" />
}

/* ── Bar Meter ───────────────────────────────────────────────── */
function BarMeter({
  value,
  max,
  redZone,
  scaleLabels,
}: {
  value: number
  max: number
  redZone?: number
  scaleLabels: Array<{ value: number; danger?: boolean }>
}) {
  const pct = Math.min(Math.max(value / max, 0), 1) * 100

  return (
    <div className="bar-meter">
      <div className="bar-track">
        <div
          className="bar-fill"
          style={{ width: `${pct}%` }}
        >
          <div className="bar-glow" />
        </div>
        {redZone != null && (
          <div
            className="bar-redzone"
            style={{ left: `${(redZone / max) * 100}%` }}
          />
        )}
      </div>
      <div className="bar-scale">
        {scaleLabels.map(l => (
          <span
            key={l.value}
            className={l.danger ? 'scale-danger' : ''}
            style={{ left: `${(l.value / max) * 100}%` }}
          >
            {l.value}
          </span>
        ))}
      </div>
    </div>
  )
}

/* ── Info Card ───────────────────────────────────────────────── */
function InfoCard({
  icon,
  value,
  unit,
  label,
}: {
  icon?: string
  value: string | number
  unit: string
  label: string
}) {
  return (
    <div className="info-card">
      <div className="info-card-content">
        {icon && <span className="info-icon">{icon}</span>}
        <span className="info-value">{value}</span>
        <span className="info-unit">{unit}</span>
      </div>
      <div className="info-label">{label}</div>
    </div>
  )
}

/* ── Action Button ───────────────────────────────────────────── */
function ActionButton({
  icon,
  label,
  onClick,
}: {
  icon: string
  label: string
  onClick: () => void
}) {
  return (
    <button className="action-btn" onClick={onClick}>
      <span className="action-icon">{icon}</span>
      <span className="action-label">{label}</span>
    </button>
  )
}

/* ── Main Dashboard ──────────────────────────────────────────── */
export default function Dashboard({ state, onAction }: Props) {
  return (
    <div className="dashboard-wrapper">
      <div className="dashboard-container">
        {/* Background image */}
        <img
          src="/dashboard-bg.webp"
          alt=""
          className="dashboard-bg"
          draggable={false}
        />

        {/* ═══ OVERLAY LAYER ═══ */}
        <div className="overlay-layer">

          {/* ── Header ── */}
          <div className="overlay-zone header-ip">
            192.168.0.55 <span className="wifi-icon">&#x1F4F6;</span>
          </div>

          {/* ── Output Power Value ── */}
          <div className="overlay-zone output-value">
            <span className="output-number">{Math.round(state.outputPower)}</span>
            <span className="output-unit">W</span>
          </div>

          {/* ── Main Output Bar ── */}
          <div className="overlay-zone output-bar-zone">
            <BarMeter
              value={state.outputPower}
              max={1500}
              redZone={1200}
              scaleLabels={[
                { value: 0 }, { value: 5 }, { value: 20 }, { value: 50 },
                { value: 100 }, { value: 400 }, { value: 600 },
                { value: 800 }, { value: 1000 }, { value: 1200, danger: true },
              ]}
            />
          </div>

          {/* ── Spectrum Visualization ── */}
          <div className="overlay-zone spectrum-zone">
            <SpectrumCanvas power={state.outputPower} />
          </div>

          {/* ── Reflected Power ── */}
          <div className="overlay-zone reflected-zone">
            <div className="small-meter-header">
              <span className="small-meter-label">Reflected</span>
              <span className="small-meter-value">{Math.round(state.reflectedPower)}</span>
              <span className="small-meter-unit">W</span>
            </div>
            <BarMeter
              value={state.reflectedPower}
              max={60}
              redZone={50}
              scaleLabels={[
                { value: 5 }, { value: 20 }, { value: 50, danger: true },
              ]}
            />
          </div>

          {/* ── Input Power ── */}
          <div className="overlay-zone input-zone">
            <div className="small-meter-header">
              <span className="small-meter-label">Input</span>
              <span className="small-meter-value">{Math.round(state.inputPower)}</span>
              <span className="small-meter-unit">W</span>
            </div>
            <BarMeter
              value={state.inputPower}
              max={120}
              redZone={100}
              scaleLabels={[
                { value: 5 }, { value: 20 }, { value: 50 },
                { value: 100, danger: true },
              ]}
            />
          </div>

          {/* ── Current ── */}
          <div className="overlay-zone current-zone">
            <div className="small-meter-header">
              <span className="small-meter-label">Current</span>
              <span className="small-meter-value">{Math.round(state.current)}</span>
              <span className="small-meter-unit">A</span>
            </div>
            <BarMeter
              value={state.current}
              max={45}
              redZone={40}
              scaleLabels={[
                { value: 10 }, { value: 20 }, { value: 30 },
                { value: 40, danger: true },
              ]}
            />
          </div>

          {/* ── Status Row ── */}
          <div className="overlay-zone status-row">
            <span className={`status-item ${state.statusOk ? 'status-ok' : 'status-err'}`}>
              Status {state.statusOk ? 'OK' : 'ERR'}
            </span>
            <span className={`status-item on-air ${state.onAir ? 'active' : ''}`}>
              ON AIR
            </span>
            <span className={`status-item ${state.fanAuto ? 'fan-auto' : ''}`}>
              Fan {state.fanAuto ? 'Auto' : 'Manual'}
            </span>
          </div>

          {/* ── Info Cards Row ── */}
          <div className="overlay-zone info-row">
            <InfoCard icon="📡" value={state.antenna} unit="" label="Antenna" />
            <InfoCard value={state.band} unit="" label="Band" />
            <InfoCard icon="⚡" value={state.swr.toFixed(2)} unit="" label="SWR" />
            <InfoCard icon="⚡" value={Math.round(state.efficiency)} unit="%" label="Eff %" />
            <InfoCard icon="⚡" value={state.voltage.toFixed(1)} unit="V" label="Volts+" />
            <InfoCard icon="🌡" value={state.temperature.toFixed(1)} unit="°C" label="Temp °C" />
          </div>

          {/* ── Action Buttons Row ── */}
          <div className="overlay-zone action-row">
            <ActionButton icon="↻" label="Reset" onClick={() => onAction('reset')} />
            <ActionButton icon="☽" label="Sleep" onClick={() => onAction('sleep')} />
            <ActionButton icon="⇥" label="ByPass" onClick={() => onAction('bypass')} />
            <ActionButton icon="✕" label="Cooling" onClick={() => onAction('cooling')} />
            <ActionButton icon="⚙" label="Setup" onClick={() => onAction('setup1')} />
            <ActionButton icon="⚙" label="Setup" onClick={() => onAction('setup2')} />
          </div>

        </div>
      </div>
    </div>
  )
}
