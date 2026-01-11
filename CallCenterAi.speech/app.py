"""
CallCenter AI - Sistema Inteligente de Análisis de Llamadas
Copyright (c) 2026 - Todos los derechos reservados
Uso no autorizado está estrictamente prohibido
"""

import whisper
from fastapi import FastAPI, UploadFile, File

app = FastAPI()
model = whisper.load_model("base")

@app.post("/transcribe")
async def transcribe(audio: UploadFile = File(...)):
    file_path = f"/tmp/{audio.filename}"
    
    with open(file_path, "wb") as f:
        f.write(await audio.read())
    
    result = model.transcribe(file_path, language="es")
    
    return {
        "text": result["text"]
    }
