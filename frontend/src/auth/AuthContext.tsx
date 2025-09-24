import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { api, setAuthToken } from '../api/client';

type AuthContextType = {
  token?: string;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | undefined>(() => localStorage.getItem('token') || undefined);

  useEffect(() => {
    setAuthToken(token);
    if (token) localStorage.setItem('token', token); else localStorage.removeItem('token');
  }, [token]);

  const login = async (username: string, password: string) => {
    const { data } = await api.post('/auth/login', { username, password });
    const tok = data.accessToken ?? data.token;
    setToken(tok);
    setAuthToken(tok);
  };

  const logout = () => setToken(undefined);

  const value = useMemo(() => ({ token, login, logout }), [token]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}


