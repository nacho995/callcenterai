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
      // Solicitar audio de alta calidad
      const stream = await navigator.mediaDevices.getUserMedia({ 
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true,
          sampleRate: 48000  // Alta calidad
        } 
      })
      
      // Configurar MediaRecorder con alta calidad
      const options = { audioBitsPerSecond: 128000 } // 128 kbps
      mediaRecorderRef.current = new MediaRecorder(stream, options)
      audioChunksRef.current = []
      setHasAudio(false)
      
      console.log('🎙️ MediaRecorder iniciado con:', mediaRecorderRef.current.mimeType)

      mediaRecorderRef.current.ondataavailable = (event) => {
        if (event.data && event.data.size > 0) {
          audioChunksRef.current.push(event.data)
          console.log('📦 Audio chunk received:', event.data.size, 'bytes')
        } else {
          console.warn('⚠️  Empty audio chunk received')
        }
      }
      
      mediaRecorderRef.current.onerror = (event) => {
        console.error('❌ MediaRecorder error:', event.error)
        setError('Error al grabar audio: ' + event.error)
      }

      // Capturar chunks cada 100ms para no perder datos
      mediaRecorderRef.current.start(100)
      setIsRecording(true)
      setError(null)
      setRecordingTime(0)
      
      console.log('✅ Recording started successfully')

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
          const chunks = audioChunksRef.current.length
          console.log('🛑 Recording stopped, chunks collected:', chunks)
          
          if (chunks === 0) {
            console.error('❌ ERROR: No audio chunks were recorded!')
            setError('No se grabó audio. Por favor, intenta de nuevo.')
            resolve(null)
            return
          }
          
          // Usar el tipo MIME que el navegador realmente está grabando
          const mimeType = mediaRecorderRef.current.mimeType || 'audio/webm'
          const audioBlob = new Blob(audioChunksRef.current, { type: mimeType })
          
          const durationSeconds = recordingTime
          console.log('📊 Total audio blob size:', audioBlob.size, 'bytes', `(${(audioBlob.size / 1024).toFixed(1)} KB)`)
          console.log('⏱️ Recording duration:', durationSeconds, 'seconds')
          
          if (audioBlob.size < 10000) {
            console.warn('⚠️  WARNING: Audio file is very small, might be too short')
          }
          
          if (durationSeconds > 0) {
            const bitrate = (audioBlob.size * 8 / durationSeconds / 1000).toFixed(0)
            console.log('📈 Bitrate aproximado:', bitrate, 'kbps')
          }
          
          // Determinar extensión basada en el tipo MIME
          const extension = mimeType.includes('webm') ? 'webm' : 
                           mimeType.includes('ogg') ? 'ogg' : 
                           mimeType.includes('mp4') ? 'mp4' : 
                           mimeType.includes('wav') ? 'wav' : 'webm'
          
          const audioFile = new File([audioBlob], `recording.${extension}`, { type: mimeType })
          console.log('✅ Audio file created:', audioFile.name, mimeType, audioFile.size, 'bytes')
          
          setHasAudio(true)
          resolve(audioFile)
        }

        // Request final data before stopping
        mediaRecorderRef.current.requestData()
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
      console.log('⏹️  Stopping recording before submit...')
      audioFile = await stopRecording()
    } else {
      // Usar el tipo MIME que se grabó originalmente
      const mimeType = mediaRecorderRef.current?.mimeType || 'audio/webm'
      const audioBlob = new Blob(audioChunksRef.current, { type: mimeType })
      const extension = mimeType.includes('webm') ? 'webm' : 
                       mimeType.includes('ogg') ? 'ogg' : 'webm'
      audioFile = new File([audioBlob], `recording.${extension}`, { type: mimeType })
    }
    
    // Validar que tenemos un archivo válido
    if (!audioFile || audioFile.size === 0) {
      console.error('❌ ERROR: No valid audio file to submit')
      setError('No se pudo crear el archivo de audio. Por favor, graba de nuevo.')
      return
    }
    
    // Validar tamaño mínimo (10KB)
    const minSize = 10 * 1024
    if (audioFile.size < minSize) {
      console.error(`❌ ERROR: Audio file too small (${audioFile.size} bytes, min: ${minSize})`)
      setError('El audio es demasiado corto. Por favor, graba al menos 2-3 segundos.')
      return
    }

    console.log('📤 Submitting audio to API:', audioFile.name, audioFile.size, 'bytes')
    
    setLoading(true)
    setError(null)
    setResult(null)

    const formData = new FormData()
    formData.append('audio', audioFile)
    formData.append('employeeId', employeeId)

    try {
      console.log(`🌐 Sending POST to ${API_URL}/api/calls/audio`)
      const startTime = Date.now()
      
      const response = await fetch(`${API_URL}/api/calls/audio`, {
        method: 'POST',
        body: formData,
      })
      
      const elapsed = ((Date.now() - startTime) / 1000).toFixed(2)
      console.log(`⏱️  API response time: ${elapsed}s`)

      if (!response.ok) {
        const errorText = await response.text()
        console.error(`❌ API Error (${response.status}):`, errorText)
        throw new Error(`Error ${response.status}: ${errorText || 'Error al procesar el audio'}`)
      }

      const data = await response.json()
      console.log('✅ API Response received:')
      console.log('   📍 Airport:', data.airport?.name, `(${data.airport?.code})`)
      console.log('   📂 Category:', data.category?.name)
      console.log('   📝 Transcript length:', data.transcript?.length, 'chars')
      console.log('   📄 Summary length:', data.summary?.length, 'chars')
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
                <span className="value">{result.category?.name || result.category || 'Sin categoría'}</span>
              </div>

              <div className="result-item full-width">
                <span className="label">Transcripción</span>
                <p className="transcript">{result.transcript}</p>
              </div>

              <div className="result-item full-width">
                <span className="label">Resumen</span>
                <p className="summary">{result.summary || 'Sin resumen disponible'}</p>
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
