import { useState } from 'react';
import { useAuth } from '../auth/AuthContext';

export default function Login() {
  const { login } = useAuth();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('admin123');
  const [error, setError] = useState<string | undefined>();

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);
    try {
      await login(username, password);
      window.location.href = '/';
    } catch (err: any) {
      setError(err?.response?.data?.detail || 'Falha no login');
    }
  };

  return (
    <div style={{ maxWidth: 360, margin: '60px auto' }}>
      <h2>Login</h2>
      <form onSubmit={submit}>
        <div>
          <label>Usuário</label>
          <input value={username} onChange={(e) => setUsername(e.target.value)} />
        </div>
        <div>
          <label>Senha</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <button type="submit">Entrar</button>
      </form>
    </div>
  );
}


