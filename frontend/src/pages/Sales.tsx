import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';
import Toast from '../components/Toast';

type Apartment = { id: string; code: string };
type Client = { id: string; name: string };

export default function Sales() {
  const [apartments, setApartments] = useState<Apartment[]>([]);
  const [clients, setClients] = useState<Client[]>([]);
  const [form, setForm] = useState({ clientId: '', apartmentId: '', downPayment: 0, totalPrice: 0 });
  const [msg, setMsg] = useState<string | undefined>();

  useEffect(() => {
    (async () => {
      const a = await api.get('/apartments', { params: { page: 1, pageSize: 50 } });
      setApartments(a.data.items.map((x: any) => ({ id: x.id, code: x.code })));
      const c = await api.get('/clients', { params: { page: 1, pageSize: 50 } });
      setClients(c.data.items.map((x: any) => ({ id: x.id, name: x.name })));
    })();
  }, []);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMsg(undefined);
    try {
      await api.post('/sales', form);
      setMsg('Venda confirmada!');
    } catch (e: any) {
      setMsg(e?.response?.data?.detail || 'Erro ao confirmar venda');
    }
  };

  return (
    <>
      <NavBar />
      <div className="container">
        <div className="panel">
          <h2 style={{ marginTop: 0 }}>Confirmar Venda</h2>
          <form onSubmit={submit} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <label>Cliente</label>
            <label>Apartamento</label>
            <select value={form.clientId} onChange={(e) => setForm({ ...form, clientId: e.target.value })} required>
              <option value="">Selecione o cliente</option>
              {clients.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select value={form.apartmentId} onChange={(e) => setForm({ ...form, apartmentId: e.target.value })} required>
              <option value="">Selecione o apartamento</option>
              {apartments.map(a => <option key={a.id} value={a.id}>{a.code}</option>)}
            </select>
            <input type="number" step="0.01" placeholder="Entrada (R$)" value={form.downPayment || ''} onChange={(e) => setForm({ ...form, downPayment: Number(e.target.value) })} />
            <input type="number" step="0.01" placeholder="Preço Total (R$)" value={form.totalPrice || ''} onChange={(e) => setForm({ ...form, totalPrice: Number(e.target.value) })} />
            <div>
              <button className="btn" type="submit">Confirmar</button>
            </div>
          </form>
          <Toast message={msg} />
        </div>
      </div>
    </>
  );
}


