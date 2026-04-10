import { useState, useEffect } from 'react'
import Dashboard from './components/Dashboard'
import './App.css'

export interface AmpState {
  outputPower: number
  reflectedPower: number
  inputPower: number
  current: number
  antenna: number
  band: string
  swr: number
  efficiency: number
  voltage: number
  temperature: number
  statusOk: boolean
  onAir: boolean
  fanAuto: boolean
}

const defaultState: AmpState = {
  outputPower: 1175,
  reflectedPower: 5,
  inputPower: 28,
  current: 345,
  antenna: 1,
  band: '30 M',
  swr: 1.14,
  efficiency: 75,
  voltage: 46.9,
  temperature: 27,
  statusOk: true,
  onAir: true,
  fanAuto: true,
}

function App() {
  const [state, setState] = useState<AmpState>(defaultState)

  // Demo: simulate live data changes
  useEffect(() => {
    const interval = setInterval(() => {
      setState(prev => ({
        ...prev,
        outputPower: prev.outputPower + (Math.random() - 0.5) * 20,
        reflectedPower: Math.max(0, prev.reflectedPower + (Math.random() - 0.5) * 2),
        inputPower: Math.max(0, prev.inputPower + (Math.random() - 0.5) * 3),
        current: Math.max(0, prev.current + (Math.random() - 0.5) * 5),
        temperature: Math.max(0, prev.temperature + (Math.random() - 0.5) * 0.5),
        voltage: Math.max(0, prev.voltage + (Math.random() - 0.5) * 0.3),
        swr: Math.max(1, prev.swr + (Math.random() - 0.5) * 0.05),
        efficiency: Math.min(100, Math.max(0, prev.efficiency + (Math.random() - 0.5) * 1)),
      }))
    }, 500)
    return () => clearInterval(interval)
  }, [])

  const handleAction = (action: string) => {
    console.log('Action:', action)
  }

  return <Dashboard state={state} onAction={handleAction} />
}

export default App
