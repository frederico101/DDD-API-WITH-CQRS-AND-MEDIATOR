import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';

type Apartment = { id: string; code: string; block: string; floor: number; number: number; price: number; status: number };
type Paged<T> = { total: number; items: T[] };

export default function Apartments() {
  const [data, setData] = useState<Paged<Apartment>>({ total: 0, items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    (async () => {
      try {
        const { data } = await api.get<Paged<Apartment>>('/apartments', { params: { page: 1, pageSize: 20 } });
        setData(data);
      } catch (e: any) {
        setError(e?.response?.data?.detail || 'Erro ao carregar');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const reserve = async (apartmentId: string) => {
    try {
      // get any client
      const clients = await api.get('/clients', { params: { page: 1, pageSize: 1 } });
      const clientId = clients.data.items[0].id;
      await api.post('/reservations', { clientId, apartmentId, expiresHours: 24 });
      alert('Reserva criada!');
    } catch (e: any) {
      alert(e?.response?.data?.detail || 'Erro ao reservar');
    }
  };

  if (loading) return <p>Carregando...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;

  return (
    <>
      <NavBar />
      <div className="container">
      <div className="panel">
      <h2 style={{ marginTop: 0 }}>Apartamentos</h2>
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Bloco</th>
            <th>Andar</th>
            <th>Número</th>
            <th>Preço</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {data.items.map((a) => (
            <tr key={a.id}>
              <td>{a.code}</td>
              <td>{a.block}</td>
              <td>{a.floor}</td>
              <td>{a.number}</td>
              <td>{a.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
              <td>
                <button className="btn" onClick={() => reserve(a.id)} disabled={a.status !== 0}>Reservar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
      </div>
    </>
  );
}


