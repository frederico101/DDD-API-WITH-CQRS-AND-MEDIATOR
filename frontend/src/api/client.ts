import axios from 'axios';

const baseURL = import.meta.env.VITE_API_URL || '/api';

export const api = axios.create({
  baseURL,
});

export function setAuthToken(token?: string) {
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  } else {
    delete api.defaults.headers.common['Authorization'];
  }
}

// Ensure Authorization header exists before any component mounts (avoids first 401)
try {
  const existing = typeof window !== 'undefined' ? localStorage.getItem('token') || undefined : undefined;
  if (existing) setAuthToken(existing);
} catch {}


