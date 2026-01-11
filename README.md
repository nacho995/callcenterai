# 🛫 CallCenter AI

Sistema inteligente de análisis de llamadas con transcripción automática y clasificación por IA.

## 🚀 Características

- 🎤 **Grabación de audio** directo desde el navegador
- 📝 **Transcripción automática** con Whisper AI
- 🤖 **Análisis inteligente** con GPT-4
- ✈️ **Clasificación por aeropuerto** y categoría
- 📊 **Resúmenes diarios** automáticos
- 💾 **Base de datos** PostgreSQL/SQLite

## 🏗️ Arquitectura

```
┌─────────────────┐
│  React Frontend │
│  (Vite + React) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│   .NET API      │─────▶│  PostgreSQL  │
│   (EF Core)     │      │   Database   │
└────────┬────────┘      └──────────────┘
         │
         ▼
┌─────────────────┐
│ Python Service  │
│    (Whisper)    │
└─────────────────┘
```

## 🛠️ Stack Tecnológico

### Backend API
- .NET 10.0
- Entity Framework Core
- PostgreSQL / SQLite
- OpenAI API (GPT-4)

### Servicio de Transcripción
- Python 3.11
- FastAPI
- OpenAI Whisper
- FFmpeg

### Frontend
- React 19
- Vite
- CSS moderno

## 🚀 Inicio Rápido

### Prerequisitos

- .NET 10 SDK
- Python 3.11+
- Node.js 20+
- PostgreSQL (opcional, usa SQLite por defecto)

### Instalación Local

**1. Clonar el repositorio**
```bash
git clone https://github.com/TU_USUARIO/callcenterai.git
cd callcenterai
```

**2. Configurar Backend .NET**
```bash
cd CallCenterAI.Api
dotnet restore
dotnet ef database update
dotnet run
```
API corriendo en: http://localhost:5284

**3. Configurar Servicio Python**
```bash
cd ../CallCenterAi.speech
python -m venv .venv
source .venv/bin/activate  # En Windows: .venv\Scripts\activate
pip install -r requirements.txt
uvicorn app:app --reload --port 8000
```
Speech service corriendo en: http://localhost:8000

**4. Configurar Frontend**
```bash
cd ../callcenterai-frontend
npm install
npm run dev
```
Frontend corriendo en: http://localhost:5173

**5. Configurar Variables de Entorno**

Crea `CallCenterAI.Api/appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "tu-api-key-aqui",
    "Model": "gpt-4o-mini"
  },
  "SpeechService": {
    "BaseUrl": "http://localhost:8000"
  }
}
```

## 📦 Deploy en Producción

Ver guía completa en [DEPLOY.md](./DEPLOY.md)

**Resumen rápido:**
1. Crear proyecto en Railway.app
2. Conectar repositorio GitHub
3. Crear 3 servicios (API, Speech, Frontend)
4. Agregar PostgreSQL
5. Configurar variables de entorno
6. Deploy automático

## 📁 Estructura del Proyecto

```
callcenterai/
├── CallCenterAI.Api/          # Backend .NET
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Data/
│   ├── Migrations/
│   └── Dockerfile
├── CallCenterAi.speech/       # Servicio Python Whisper
│   ├── app.py
│   ├── requirements.txt
│   └── Dockerfile
├── callcenterai-frontend/     # Frontend React
│   ├── src/
│   ├── public/
│   ├── Dockerfile
│   └── nginx.conf
├── railway.json               # Config Railway
├── DEPLOY.md                  # Guía de deploy
└── README.md
```

## 🔑 Variables de Entorno

### API (.NET)
```bash
ConnectionStrings__DefaultConnection=postgresql://...
OpenAI__ApiKey=sk-proj-...
OpenAI__Model=gpt-4o-mini
SpeechService__BaseUrl=http://localhost:8000
ASPNETCORE_ENVIRONMENT=Development
```

### Speech (Python)
```bash
PORT=8000
```

### Frontend (React)
```bash
VITE_API_URL=http://localhost:5284
```

## 🧪 Testing

### Backend
```bash
cd CallCenterAI.Api
dotnet test
```

### Frontend
```bash
cd callcenterai-frontend
npm test
```

## 📊 API Endpoints

### Calls
- `POST /api/calls/audio` - Analizar llamada desde audio
- `POST /api/calls` - Crear llamada desde texto
- `GET /api/calls` - Listar llamadas

### Health
- `GET /health` - Estado del servicio
- `GET /openapi/v1.json` - Documentación OpenAPI

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Copyright © 2026 - Todos los derechos reservados.

Este es un software propietario. Ver [LICENSE](./LICENSE) para más detalles.

## 👥 Autores

- Tu Nombre - [@tu_usuario](https://github.com/tu_usuario)

## 🙏 Agradecimientos

- OpenAI por GPT y Whisper
- Railway por hosting
- Comunidad .NET y React
