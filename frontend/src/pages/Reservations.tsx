import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';

type Reservation = {
  id: string;
  clientId: string;
  clientName: string;
  apartmentId: string;
  apartmentCode: string;
  expiresAtUtc?: string;
  confirmedAsSale: boolean;
};

type Paged<T> = { total: number; items: T[] };

export default function Reservations() {
  const [data, setData] = useState<Paged<Reservation>>({ total: 0, items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    (async () => {
      try {
        const { data } = await api.get<Paged<Reservation>>('/reservations', { params: { page: 1, pageSize: 20 } });
        setData(data);
      } catch (e: any) {
        setError(e?.response?.data?.detail || 'Erro ao carregar reservas');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <>
      <NavBar />
      <div className="container">
        <div className="panel">
          <h2 style={{ marginTop: 0 }}>Reservas</h2>
          {loading ? <p>Carregando...</p> : error ? <p style={{ color: 'salmon' }}>{error}</p> : (
            <table>
              <thead>
                <tr>
                  <th>Apartamento</th>
                  <th>Cliente</th>
                  <th>Expira em</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map(r => (
                  <tr key={r.id}>
                    <td>{r.apartmentCode}</td>
                    <td>{r.clientName}</td>
                    <td>{r.expiresAtUtc ? new Date(r.expiresAtUtc).toLocaleString('pt-BR') : '-'}</td>
                    <td>{r.confirmedAsSale ? 'Confirmada (Venda)' : 'Reservada'}</td>
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


