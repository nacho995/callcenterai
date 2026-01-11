# 🚀 Guía de Deploy en Render

Deploy super fácil de CallCenter AI en Render.

## 📋 Prerequisitos

1. Cuenta en [Render.com](https://render.com) (gratis)
2. Repositorio GitHub (público o privado)
3. API Key de OpenAI

## 🎯 Ventajas de Render

- ✅ **UI más intuitiva** que Railway
- ✅ **Plan gratuito** para API y Frontend
- ✅ **PostgreSQL gratis** (hasta 1GB)
- ✅ **Deploy automático** desde Git
- ⚠️ Speech Service necesita plan de pago ($7/mes mínimo por recursos)

---

## 🚀 Método 1: Deploy con 1 Click (Blueprint)

### Paso 1: Subir a GitHub

```bash
# Si no has subido el código
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git push -u origin main
```

### Paso 2: Deploy con Blueprint

1. Ve a [Render Dashboard](https://dashboard.render.com)
2. Click en **"New +"** → **"Blueprint"**
3. Conecta tu repositorio GitHub
4. Render detectará `render.yaml` automáticamente
5. Click **"Apply"**
6. Espera 5-10 minutos mientras se crean todos los servicios

### Paso 3: Agregar API Key de OpenAI

1. Ve al servicio **callcenterai-api**
2. Click en **"Environment"**
3. Busca `OpenAI__ApiKey`
4. Agrega tu API key: `sk-proj-...`
5. Click **"Save Changes"** (se redeployeará automáticamente)

### ✅ ¡Listo!

Tu app estará disponible en:
- Frontend: `https://callcenterai-frontend.onrender.com`
- API: `https://callcenterai-api.onrender.com`
- Speech: `https://callcenterai-speech.onrender.com`

---

## 🔧 Método 2: Deploy Manual (Más control)

### Paso 1: Crear PostgreSQL

1. Dashboard → **"New +"** → **"PostgreSQL"**
2. Configuración:
   - Name: `callcenterai-db`
   - Database: `callcenterai`
   - User: `callcenterai`
   - Region: **Frankfurt** (Europa)
   - Plan: **Free**
3. Click **"Create Database"**
4. Guarda la **Internal Database URL** (la usaremos después)

---

### Paso 2: Deploy Speech Service (Python/Whisper)

1. Dashboard → **"New +"** → **"Web Service"**
2. Conecta GitHub y selecciona tu repo
3. Configuración:
   - Name: `callcenterai-speech`
   - Region: **Frankfurt**
   - Branch: `main`
   - Root Directory: `CallCenterAi.speech`
   - Environment: **Docker**
   - Dockerfile Path: `./CallCenterAi.speech/Dockerfile`
   - Instance Type: **Starter** ($7/mes - necesario para Whisper)

4. Variables de entorno:
   ```
   PORT=8080
   ```

5. Click **"Create Web Service"**
6. Espera 5-10 minutos (primera vez tarda porque descarga Whisper)
7. Copia la URL del servicio (ej: `https://callcenterai-speech.onrender.com`)

---

### Paso 3: Deploy API Backend (.NET)

1. Dashboard → **"New +"** → **"Web Service"**
2. Conecta GitHub y selecciona tu repo
3. Configuración:
   - Name: `callcenterai-api`
   - Region: **Frankfurt**
   - Branch: `main`
   - Root Directory: `CallCenterAI.Api`
   - Environment: **Docker**
   - Dockerfile Path: `./CallCenterAI.Api/Dockerfile`
   - Instance Type: **Free**

4. Variables de entorno:
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   PORT=8080
   
   # Copia Internal Database URL de PostgreSQL:
   ConnectionStrings__DefaultConnection=postgresql://user:pass@host/db
   
   # Tu API key de OpenAI:
   OpenAI__ApiKey=sk-proj-tu-key-aqui
   OpenAI__Model=gpt-4o-mini
   
   # URL del servicio Speech (del paso anterior):
   SpeechService__BaseUrl=https://callcenterai-speech.onrender.com
   ```

5. Click **"Create Web Service"**
6. Espera 3-5 minutos
7. Copia la URL del servicio (ej: `https://callcenterai-api.onrender.com`)

---

### Paso 4: Deploy Frontend (React)

1. Dashboard → **"New +"** → **"Web Service"**
2. Conecta GitHub y selecciona tu repo
3. Configuración:
   - Name: `callcenterai-frontend`
   - Region: **Frankfurt**
   - Branch: `main`
   - Root Directory: `callcenterai-frontend`
   - Environment: **Docker**
   - Dockerfile Path: `./callcenterai-frontend/Dockerfile`
   - Instance Type: **Free**

4. Variables de entorno:
   ```bash
   # URL de tu API (del paso anterior):
   VITE_API_URL=https://callcenterai-api.onrender.com
   ```

5. Click **"Create Web Service"**
6. Espera 3-5 minutos

---

## ✅ Verificar Deploy

### 1. **Probar Speech Service:**
Ve a: `https://callcenterai-speech.onrender.com/docs`
Deberías ver la documentación de FastAPI.

### 2. **Probar API:**
Ve a: `https://callcenterai-api.onrender.com/openapi/v1.json`
Deberías ver el schema OpenAPI.

### 3. **Probar Frontend:**
Ve a: `https://callcenterai-frontend.onrender.com`
Deberías ver la interfaz de CallCenter AI.

---

## ⚙️ Configuración de Dominios Custom (Opcional)

1. Ve a tu servicio en Render
2. Click en **"Settings"** → **"Custom Domain"**
3. Agrega tu dominio (ej: `callcenter.tudominio.com`)
4. Configura DNS según instrucciones de Render
5. Render provee SSL automático con Let's Encrypt

---

## 💰 Costos en Render

| Servicio | Plan | Costo |
|----------|------|-------|
| PostgreSQL | Free | $0/mes (1GB) |
| API (.NET) | Free | $0/mes |
| Frontend | Free | $0/mes |
| Speech (Whisper) | Starter | $7/mes |
| **TOTAL** | | **$7/mes** |

**Servicios gratuitos:**
- Se "duermen" después de 15 min inactividad
- Primera request tarda ~30s en "despertar"
- Suficiente para demos/desarrollo

**Upgrade a Starter ($7/mes por servicio):**
- Siempre activo
- Mejor performance
- Más recursos

---

## 🔄 Redeploys Automáticos

Render redeploya automáticamente cuando haces push a GitHub:

```bash
git add .
git commit -m "Actualización"
git push
```

O redeploy manual:
1. Ve al servicio en Render
2. Click en **"Manual Deploy"** → **"Deploy latest commit"**

---

## 🐛 Troubleshooting

### Error: Speech service timeout
**Solución:** El tier Free es muy lento para Whisper. Necesitas Starter ($7/mes).

### Error: Database connection failed
**Solución:** 
1. Verifica que usaste **Internal Database URL** (no External)
2. En PostgreSQL dashboard, copia la URL completa
3. Pégala en `ConnectionStrings__DefaultConnection`

### Error: CORS en producción
**Solución:** Ya está configurado en el código con `AllowAnyOrigin()`.

### Frontend no conecta con API
**Solución:**
1. Verifica `VITE_API_URL` tenga la URL correcta
2. Debe ser la URL pública de la API
3. Redeploy el frontend después de cambiarla

### Servicio "dormido" (plan Free)
**Solución:**
- Primera request tardará ~30s
- O upgradea a Starter ($7/mes) para que esté siempre activo

---

## 📊 Monitoreo

Render provee:
- **Logs en tiempo real**: Click en "Logs" en cada servicio
- **Métricas**: CPU, memoria, requests
- **Health checks**: Verifica que servicios estén up
- **Alertas**: Email si algo falla

---

## 🔐 Variables de Entorno Resumen

### PostgreSQL (Auto-generado)
```
DATABASE_URL=postgresql://user:pass@host/db
```

### API (.NET)
```bash
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
ConnectionStrings__DefaultConnection=postgresql://...
OpenAI__ApiKey=sk-proj-...
OpenAI__Model=gpt-4o-mini
SpeechService__BaseUrl=https://callcenterai-speech.onrender.com
```

### Speech (Python)
```bash
PORT=8080
```

### Frontend (React)
```bash
VITE_API_URL=https://callcenterai-api.onrender.com
```

---

## 🎉 ¡Listo!

Tu CallCenter AI está desplegado en Render.

**URLs finales:**
- 🎨 **Frontend**: https://callcenterai-frontend.onrender.com
- 🔌 **API**: https://callcenterai-api.onrender.com  
- 🎤 **Speech**: https://callcenterai-speech.onrender.com
- 🗄️ **Database**: (interno)

---

## 📱 Próximos Pasos

1. **Prueba la aplicación completa**
2. **Configura dominio custom** (opcional)
3. **Upgradea servicios a Starter** si necesitas mejor performance
4. **Configura alertas** en Render
5. **Monitorea uso de PostgreSQL** (límite 1GB en free tier)

---

## 🆘 ¿Problemas?

Si tienes errores:
1. Revisa los **Logs** en cada servicio
2. Verifica las **variables de entorno**
3. Asegúrate que todos los servicios estén **"Live"** (verde)
4. El servicio Speech tarda ~5-10 min en primera deploy
