# 🔐 Opciones de Licencia para tu Proyecto

## 📋 Comparación de Licencias

### 1. 🔒 **PROPIETARIA / COPYRIGHT** (Actual - Máxima Protección)

**¿Qué significa?**
- Tú eres el único dueño
- Nadie puede usar, copiar o modificar sin tu permiso
- Puedes vender licencias comerciales
- Control total sobre tu código

**Ventajas:**
- ✅ Máxima protección legal
- ✅ Puedes monetizar sin restricciones
- ✅ Nadie puede "robarte" el código legalmente

**Desventajas:**
- ❌ No puedes ponerlo en GitHub público
- ❌ Dificulta colaboración open source
- ❌ Menos visibilidad para portfolio

**Cuándo usar:** Producto comercial, startup, MVP para vender

---

### 2. 📜 **MIT License** (Más Popular)

```
Copyright (c) 2026 [Tu Nombre]

Se concede permiso para usar, copiar, modificar y distribuir
con atribución al autor original.
```

**Ventajas:**
- ✅ Muestra tu trabajo en portfolio
- ✅ Empresas pueden ver tu código
- ✅ Bueno para Open Source
- ✅ Requiere dar crédito a tu nombre

**Desventajas:**
- ❌ Cualquiera puede usar tu código comercialmente
- ❌ Pueden crear productos competidores

**Cuándo usar:** Portfolio, proyectos educativos, quieres colaboración

---

### 3. 🔄 **GPL v3** (Copyleft Fuerte)

**¿Qué significa?**
- Quien use tu código DEBE hacer su proyecto también Open Source
- No pueden cerrarlo comercialmente
- Protege contra "robo" corporativo

**Ventajas:**
- ✅ Si alguien usa tu código, debe compartir sus cambios
- ✅ Evita que empresas te "roben" cerrando el código
- ✅ Bueno para comunidad Open Source

**Desventajas:**
- ❌ Empresas evitan GPL (no pueden cerrarlo)
- ❌ Menos adopción comercial

**Cuándo usar:** Quieres Open Source pero protegido

---

### 4. ⚖️ **Apache 2.0**

**Similar a MIT pero:**
- Protección adicional de patentes
- Mejor para proyectos empresariales
- Permite uso comercial con atribución

---

### 5. 🎨 **Creative Commons BY-NC-ND**

- **BY**: Requiere atribución
- **NC**: No Comercial (solo uso personal/educativo)
- **ND**: No Derivados (no pueden modificar)

**Cuándo usar:** Proyectos educativos, demos, no quieres uso comercial

---

## 🤔 ¿Cuál elegir?

### 🎯 Casos de Uso Comunes:

**1. Quiero vender este proyecto / crear una startup:**
→ **PROPIETARIA/COPYRIGHT** (actual) ✅

**2. Quiero portfolio pero que no lo copien comercialmente:**
→ **GPL v3** o **CC BY-NC**

**3. Quiero portfolio y que empresas lo vean:**
→ **MIT** o **Apache 2.0**

**4. No me importa, quiero compartir:**
→ **MIT**

---

## 🛡️ Protección Adicional (Independiente de Licencia)

### 1. **Mantén el repositorio PRIVADO**
- GitHub/GitLab/Bitbucket privado
- Solo invita colaboradores de confianza
- Es la protección más efectiva

### 2. **No subas información sensible**
- API keys → Variables de entorno
- Contraseñas → Secrets
- Lógica crítica → Ofuscar o servicios externos

### 3. **Copyright en el código**
Agrega en cada archivo importante:

```csharp
/*
 * Copyright (c) 2026 [Tu Nombre]
 * Todos los derechos reservados.
 * Uso no autorizado prohibido.
 */
```

### 4. **Registro de Copyright** (USA)
- Registra en US Copyright Office ($65)
- Protección legal más fuerte
- https://www.copyright.gov

### 5. **Términos de Servicio**
Si es una app web, agrega ToS que prohíba:
- Scraping
- Ingeniería inversa
- Uso no autorizado

---

## 📝 Cambiar de Licencia

Si quieres cambiar a MIT, GPL u otra:

**MIT:**
```bash
# Reemplazar LICENSE con:
wget https://raw.githubusercontent.com/licenses/license-templates/master/templates/mit.txt -O LICENSE
```

**GPL v3:**
```bash
wget https://www.gnu.org/licenses/gpl-3.0.txt -O LICENSE
```

---

## ⚠️ IMPORTANTE

**Una licencia NO protege si:**
- Tu repositorio es público → Todos pueden verlo
- No tienes copyright registrado → Más difícil probar autoría
- Alguien te copia en otro país → Difícil accionar legalmente

**La mejor protección es:**
1. **Repositorio PRIVADO** (GitHub/GitLab)
2. **No compartir código crítico**
3. **Lanzar producto antes que otros puedan copiarte**
4. **Construir marca y comunidad** (difícil de copiar)

---

## 💼 Recomendación para ti

Basado en "CallCenter AI":

**Opción 1 - Máxima Protección (Actual):**
- Mantén repositorio **PRIVADO**
- Licencia **PROPIETARIA** 
- Deploy solo el frontend
- API y backend privados

**Opción 2 - Portfolio + Protección:**
- Repositorio **PÚBLICO**
- Licencia **GPL v3**
- Requiere que derivados sean Open Source
- Muestra tu trabajo pero protegido

**Opción 3 - Portfolio Abierto:**
- Repositorio **PÚBLICO**
- Licencia **MIT**
- Mejor para conseguir trabajo
- Acepta que puedan copiar

---

¿Qué prefieres? ¿Mantengo la licencia propietaria o quieres cambiar?
