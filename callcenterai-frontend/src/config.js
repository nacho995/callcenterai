// Primero intentar leer de window.ENV (runtime), luego VITE, luego localhost
export const API_URL = 
  (typeof window !== 'undefined' && window.ENV?.API_URL) ||
  import.meta.env.VITE_API_URL || 
  'http://localhost:5284';

console.log('🔗 API_URL configurada:', API_URL);
