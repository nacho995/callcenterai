/*
 * CallCenter AI - Sistema Inteligente de Análisis de Llamadas
 * Copyright (c) 2026 - Todos los derechos reservados
 * Uso no autorizado está estrictamente prohibido
 */

import { useState, useRef } from 'react'
import './App.css'
import { API_URL } from './config'

function App() {
  const [employeeId, setEmployeeId] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState(null)
  const [error, setError] = useState(null)
  const [isRecording, setIsRecording] = useState(false)
  const [recordingTime, setRecordingTime] = useState(0)
  const [hasAudio, setHasAudio] = useState(false)

  const mediaRecorderRef = useRef(null)
  const audioChunksRef = useRef([])
  const timerRef = useRef(null)

  const startRecording = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      mediaRecorderRef.current = new MediaRecorder(stream)
      audioChunksRef.current = []
      setHasAudio(false)

      mediaRecorderRef.current.ondataavailable = (event) => {
        audioChunksRef.current.push(event.data)
      }

      mediaRecorderRef.current.start()
      setIsRecording(true)
      setError(null)
      setRecordingTime(0)

      // Iniciar temporizador
      timerRef.current = setInterval(() => {
        setRecordingTime(prev => prev + 1)
      }, 1000)

    } catch (err) {
      setError('Error al acceder al micrófono. Por favor, permite el acceso.')
    }
  }

  const stopRecording = () => {
    return new Promise((resolve) => {
      if (mediaRecorderRef.current && isRecording) {
        mediaRecorderRef.current.onstop = () => {
          // Usar el tipo MIME que el navegador realmente está grabando
          const mimeType = mediaRecorderRef.current.mimeType || 'audio/webm'
          const audioBlob = new Blob(audioChunksRef.current, { type: mimeType })
          
          // Determinar extensión basada en el tipo MIME
          const extension = mimeType.includes('webm') ? 'webm' : 
                           mimeType.includes('ogg') ? 'ogg' : 
                           mimeType.includes('mp4') ? 'mp4' : 'webm'
          
          const audioFile = new File([audioBlob], `recording.${extension}`, { type: mimeType })
          console.log('🎤 Audio grabado:', audioFile.name, mimeType, audioFile.size, 'bytes')
          setHasAudio(true)
          resolve(audioFile)
        }

        mediaRecorderRef.current.stop()
        mediaRecorderRef.current.stream.getTracks().forEach(track => track.stop())
        setIsRecording(false)

        // Detener temporizador
        if (timerRef.current) {
          clearInterval(timerRef.current)
          timerRef.current = null
        }
      }
    })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!employeeId) {
      setError('Por favor, ingresa tu ID de empleado')
      return
    }

    if (!isRecording && !hasAudio) {
      setError('Por favor, graba un audio primero')
      return
    }

    let audioFile = null
    if (isRecording) {
      audioFile = await stopRecording()
    } else {
      // Usar el tipo MIME que se grabó originalmente
      const mimeType = mediaRecorderRef.current?.mimeType || 'audio/webm'
      const audioBlob = new Blob(audioChunksRef.current, { type: mimeType })
      const extension = mimeType.includes('webm') ? 'webm' : 
                       mimeType.includes('ogg') ? 'ogg' : 'webm'
      audioFile = new File([audioBlob], `recording.${extension}`, { type: mimeType })
    }

    setLoading(true)
    setError(null)
    setResult(null)

    const formData = new FormData()
    formData.append('audio', audioFile)
    formData.append('employeeId', employeeId)

    try {
      const response = await fetch(`${API_URL}/api/calls/audio`, {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        throw new Error('Error al procesar el audio')
      }

      const data = await response.json()
      setResult(data)
      audioChunksRef.current = []
      setRecordingTime(0)
      setHasAudio(false)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${mins}:${secs.toString().padStart(2, '0')}`
  }

  return (
    <div className="app">
      <div className="container">
        <header>
          <h1>🛫 Call Center AI</h1>
          <p>Sistema inteligente de análisis de llamadas</p>
        </header>

        <div className="card">
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="employeeId">ID del Empleado</label>
              <input
                type="text"
                id="employeeId"
                value={employeeId}
                onChange={(e) => setEmployeeId(e.target.value)}
                placeholder="Ej: EMP001"
                disabled={loading || isRecording}
              />
            </div>

            <div className="form-group">
              <label>Grabación de Audio</label>
              <div className="recording-controls">
                {!isRecording && !hasAudio && (
                  <button
                    type="button"
                    onClick={startRecording}
                    className="btn-record"
                    disabled={loading}
                  >
                    🎤 Iniciar Grabación
                  </button>
                )}

                {isRecording && (
                  <div className="recording-active">
                    <div className="recording-indicator">
                      <span className="recording-dot"></span>
                      <span className="recording-text">Grabando... {formatTime(recordingTime)}</span>
                    </div>
                    <button
                      type="button"
                      onClick={stopRecording}
                      className="btn-stop"
                    >
                      ⏹️ Detener
                    </button>
                  </div>
                )}

                {!isRecording && hasAudio && (
                  <div className="recording-ready">
                    <span className="ready-text">✅ Audio listo ({formatTime(recordingTime)})</span>
                    <button
                      type="button"
                      onClick={startRecording}
                      className="btn-rerecord"
                      disabled={loading}
                    >
                      🔄 Volver a grabar
                    </button>
                  </div>
                )}
              </div>
            </div>

            <button
              type="submit"
              disabled={loading || isRecording || !hasAudio}
              className="btn-primary"
            >
              {loading ? '⏳ Procesando...' : '🚀 Analizar Llamada'}
            </button>
          </form>
        </div>

        {error && (
          <div className="alert alert-error">
            ❌ {error}
          </div>
        )}

        {result && (
          <div className="card result-card">
            <h2>✅ Análisis Completado</h2>

            <div className="result-grid">
              <div className="result-item">
                <span className="label">Aeropuerto</span>
                <span className="value">{result.airport?.code || 'N/A'} - {result.airport?.name || 'N/A'}</span>
              </div>

              <div className="result-item">
                <span className="label">Categoría</span>
                <span className="value">{result.category?.name || 'N/A'}</span>
              </div>

              <div className="result-item full-width">
                <span className="label">Transcripción</span>
                <p className="transcript">{result.transcript}</p>
              </div>

              <div className="result-item full-width">
                <span className="label">Resumen</span>
                <p className="summary">{result.summary}</p>
              </div>

              <div className="result-item">
                <span className="label">ID de la Llamada</span>
                <span className="value">#{result.id}</span>
              </div>

              <div className="result-item">
                <span className="label">Fecha</span>
                <span className="value">{new Date(result.createdAt).toLocaleString('es-ES')}</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export default App
