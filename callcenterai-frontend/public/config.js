// Este archivo se carga en runtime y puede leer variables del window
window.ENV = {
  API_URL: window.location.origin.includes('localhost') 
    ? 'http://localhost:5284'
    : 'https://callcenterai-1.onrender.com'
};
