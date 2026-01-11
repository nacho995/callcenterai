# 🚀 Guía de Deploy en Railway

Esta guía te ayudará a desplegar CallCenter AI en Railway con 3 servicios separados.

## 📋 Prerequisitos

1. Cuenta en [Railway.app](https://railway.app)
2. Git instalado
3. Repositorio GitHub (público o privado)

## 🏗️ Arquitectura

El proyecto se compone de 3 servicios:

- **API (.NET)**: Backend principal en puerto 8080
- **Speech (Python)**: Servicio de transcripción Whisper en puerto 8080
- **Frontend (React)**: Interfaz web en puerto 80

## 📦 Paso 1: Preparar el Repositorio

```bash
# Inicializar git si no lo has hecho
git init

# Agregar todos los archivos
git add .
git commit -m "Preparar proyecto para deploy en Railway"

# Crear repositorio en GitHub y subir
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git branch -M main
git push -u origin main
```

## 🚂 Paso 2: Crear Proyecto en Railway

1. Ve a [Railway.app](https://railway.app) y haz login
2. Click en "New Project"
3. Selecciona "Deploy from GitHub repo"
4. Autoriza Railway y selecciona tu repositorio

## 🗄️ Paso 3: Crear Base de Datos PostgreSQL

1. En tu proyecto de Railway, click en "+ New"
2. Selecciona "Database" → "PostgreSQL"
3. Espera a que se provisione (1-2 minutos)

## ⚙️ Paso 4: Configurar Servicio API (.NET)

1. Click en "+ New" → "GitHub Repo" → Selecciona tu repo
2. Configura:
   - **Name**: `callcenterai-api`
   - **Root Directory**: `/CallCenterAI.Api`
   - **Builder**: Dockerfile

3. Agrega las siguientes **Variables de Entorno**:

```bash
# Railway genera automáticamente DATABASE_URL
# Necesitas crear estas:

ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
OpenAI__ApiKey=tu-api-key-de-openai
OpenAI__Model=gpt-4o-mini
SpeechService__BaseUrl=https://tu-speech-service.railway.app
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

4. Click en "Deploy"

## 🎤 Paso 5: Configurar Servicio Speech (Python)

1. Click en "+ New" → "GitHub Repo" → Selecciona tu repo
2. Configura:
   - **Name**: `callcenterai-speech`
   - **Root Directory**: `/CallCenterAi.speech`
   - **Builder**: Dockerfile

3. Agrega variable de entorno:

```bash
PORT=8080
```

4. **IMPORTANTE**: En Settings → Deploy:
   - Memory: Mínimo 2GB (Whisper consume memoria)
   - CPU: Mínimo 2 vCPU

5. Click en "Deploy"

## 🎨 Paso 6: Configurar Frontend (React)

1. Click en "+ New" → "GitHub Repo" → Selecciona tu repo
2. Configura:
   - **Name**: `callcenterai-frontend`
   - **Root Directory**: `/callcenterai-frontend`
   - **Builder**: Dockerfile

3. Agrega variable de entorno:

```bash
VITE_API_URL=https://tu-api-service.railway.app
PORT=80
```

4. Click en "Deploy"

## 🔗 Paso 7: Conectar los Servicios

1. Ve al servicio **API**
2. En Settings → Networking, copia la URL pública
3. Ve al **Frontend** → Variables
4. Actualiza `VITE_API_URL` con la URL de la API

5. Ve al servicio **Speech**
6. En Settings → Networking, copia la URL pública
7. Ve a la **API** → Variables
8. Actualiza `SpeechService__BaseUrl` con la URL del servicio Speech

9. Redeploy todos los servicios para aplicar cambios

## ✅ Paso 8: Verificar Deploy

Espera a que todos los servicios muestren "Active" (puede tardar 5-10 min).

Prueba:
- Frontend: Abre la URL pública del frontend
- API: `https://tu-api.railway.app/openapi/v1.json`
- Speech: `https://tu-speech.railway.app/docs`

## 🎯 Configuración de Dominios (Opcional)

1. En cada servicio, ve a Settings → Domains
2. Puedes agregar dominios custom o usar los de Railway
3. Actualiza las variables de entorno con los nuevos dominios

## 💰 Costos Estimados

Railway cobra por uso:

- **API .NET**: ~$5-10/mes (uso ligero)
- **Speech Python**: ~$15-30/mes (Whisper consume recursos)
- **Frontend**: ~$1-3/mes (solo nginx)
- **PostgreSQL**: Gratis hasta 512MB

**Total estimado**: $20-45/mes

## 🔧 Troubleshooting

### Error: No se conecta a PostgreSQL
- Verifica que `ConnectionStrings__DefaultConnection` esté configurada
- Asegúrate de usar `${{Postgres.DATABASE_URL}}`

### Error: Speech service timeout
- Aumenta memoria a 2GB mínimo
- Whisper tarda ~30s en cargar por primera vez

### Error: CORS en producción
- Verifica que la URL del frontend esté en CORS de la API
- O deja `AllowAnyOrigin()` para simplificar

## 🚀 Redeploys

Railway redeploys automáticamente cuando haces push a GitHub:

```bash
git add .
git commit -m "Actualización"
git push
```

## 📝 Variables de Entorno Resumen

### API (.NET)
```
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
OpenAI__ApiKey=sk-proj-...
OpenAI__Model=gpt-4o-mini
SpeechService__BaseUrl=https://speech-service.railway.app
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

### Speech (Python)
```
PORT=8080
```

### Frontend (React)
```
VITE_API_URL=https://api-service.railway.app
```

## 🎉 ¡Listo!

Tu aplicación CallCenter AI está ahora en producción y lista para usarse.

**URLs finales:**
- 🎨 Frontend: https://callcenterai-frontend.railway.app
- 🔌 API: https://callcenterai-api.railway.app
- 🎤 Speech: https://callcenterai-speech.railway.app
