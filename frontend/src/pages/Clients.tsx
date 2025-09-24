import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';

type Client = { id: string; name: string; email: string; document: string; phone: string };
type Paged<T> = { total: number; items: T[] };

export default function Clients() {
  const [list, setList] = useState<Paged<Client>>({ total: 0, items: [] });
  const [form, setForm] = useState({ name: '', email: '', document: '', phone: '' });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await api.get<Paged<Client>>('/clients', { params: { page: 1, pageSize: 20 } });
      setList(data);
    } catch (e: any) {
      setError(e?.response?.data?.detail || 'Erro ao carregar');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);
    try {
      await api.post('/clients', form);
      setForm({ name: '', email: '', document: '', phone: '' });
      await load();
    } catch (e: any) {
      setError(e?.response?.data?.detail || 'Erro ao salvar');
    }
  };

  return (
    <>
      <NavBar />
      <div className="container">
        <div className="panel">
          <h2 style={{ marginTop: 0 }}>Cadastro de Clientes</h2>
          <form onSubmit={submit} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginBottom: 16 }}>
            <input placeholder="Nome" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            <input placeholder="Documento" value={form.document} onChange={(e) => setForm({ ...form, document: e.target.value })} required />
            <input placeholder="Telefone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            <div>
              <button className="btn" type="submit">Salvar</button>
            </div>
          </form>
          {error && <p style={{ color: 'salmon' }}>{error}</p>}
          {loading ? <p>Carregando...</p> : (
            <table>
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Documento</th>
                  <th>Telefone</th>
                </tr>
              </thead>
              <tbody>
                {list.items.map(c => (
                  <tr key={c.id}>
                    <td>{c.name}</td>
                    <td>{c.email}</td>
                    <td>{c.document}</td>
                    <td>{c.phone}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </>
  );
}


