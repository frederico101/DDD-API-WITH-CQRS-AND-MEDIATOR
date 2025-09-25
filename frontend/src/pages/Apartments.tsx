import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';
import Modal from '../components/Modal';
import { TableSkeleton } from '../components/Skeleton';
import ApartmentImage from '../components/ApartmentImage';

type Apartment = { id: string; code: string; block: string; floor: number; number: number; price: number; status: number };
type Reservation = { id: string; clientId: string; clientName: string; apartmentId: string; apartmentCode: string; expiresAtUtc?: string; confirmedAsSale: boolean };
type Paged<T> = { total: number; items: T[] };

export default function Apartments() {
  const [data, setData] = useState<Paged<Apartment>>({ total: 0, items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Apartment | null>(null);
  const [clients, setClients] = useState<{ id: string; name: string }[]>([]);
  const [toast, setToast] = useState<string | undefined>();
  const [clientId, setClientId] = useState('');
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [hoveredApartmentId, setHoveredApartmentId] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const [apartmentsRes, reservationsRes] = await Promise.all([
          api.get<Paged<Apartment>>('/apartments', { params: { page: 1, pageSize: 20 } }),
          api.get<Paged<Reservation>>('/reservations', { params: { page: 1, pageSize: 50 } })
        ]);
        setData(apartmentsRes.data);
        setReservations(reservationsRes.data.items);
      } catch (e: any) {
        setError(e?.response?.data?.detail || 'Erro ao carregar');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const openReserve = async (a: Apartment) => {
    setSelected(a);
    const res = await api.get('/clients', { params: { page: 1, pageSize: 50 } });
    setClients(res.data.items.map((x: any) => ({ id: x.id, name: x.name })));
    setClientId('');
    setOpen(true);
  };

  const submitReserve = async () => {
    if (!selected || !clientId) return;
    try {
      await api.post('/reservations', { clientId, apartmentId: selected.id, expiresHours: 24 });
      setOpen(false);
      setToast('Reserva criada!');
      
      // Refresh data
      const [apartmentsRes, reservationsRes] = await Promise.all([
        api.get<Paged<Apartment>>('/apartments', { params: { page: 1, pageSize: 20 } }),
        api.get<Paged<Reservation>>('/reservations', { params: { page: 1, pageSize: 50 } })
      ]);
      setData(apartmentsRes.data);
      setReservations(reservationsRes.data.items);
    } catch (e: any) {
      setToast(e?.response?.data?.detail || 'Erro ao reservar');
    }
  };

  const getReservationForApartment = (apartmentId: string) => {
    return reservations.find(r => r.apartmentId === apartmentId && !r.confirmedAsSale);
  };

  if (loading) {
    return (
      <>
        <NavBar />
        <div className="container">
          <div className="panel">
            <h2 style={{ marginTop: 0 }}>Apartamentos</h2>
            <TableSkeleton rows={8} />
          </div>
        </div>
      </>
    );
  }

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
            <th>Imagem</th>
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
              <td><ApartmentImage apartmentCode={a.code} size="small" /></td>
              <td>{a.code}</td>
              <td>{a.block}</td>
              <td>{a.floor}</td>
              <td>{a.number}</td>
              <td>{a.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
              <td>
                {a.status === 0 && (
                  <button className="btn" onClick={() => openReserve(a)}>Reservar</button>
                )}
                {a.status === 1 && (
                  <div 
                    className="tooltip-container"
                    onMouseEnter={() => setHoveredApartmentId(a.id)}
                    onMouseLeave={() => setHoveredApartmentId(null)}
                  >
                    <button className="btn btn--secondary" disabled>Reservado</button>
                    {hoveredApartmentId === a.id && (
                      <div className="tooltip">
                        {(() => {
                          const reservation = getReservationForApartment(a.id);
                          return reservation ? (
                            <>
                              <strong>Cliente:</strong> {reservation.clientName}<br/>
                              <strong>Expira em:</strong> {reservation.expiresAtUtc ? new Date(reservation.expiresAtUtc).toLocaleString('pt-BR') : 'Não definido'}
                            </>
                          ) : (
                            'Detalhes não disponíveis'
                          );
                        })()}
                      </div>
                    )}
                  </div>
                )}
                {a.status === 2 && (
                  <button className="btn btn--success" disabled>Vendido</button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
      </div>

      <Modal open={open} title="Reservar Apartamento" onClose={() => setOpen(false)}>
        <p>
          Apartamento: <strong>{selected?.code}</strong>
        </p>
        <label>Cliente</label>
        <select style={{ width: '100%', marginBottom: 12 }} value={clientId} onChange={(e) => setClientId(e.target.value)}>
          <option value="">Selecione</option>
          {clients.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button className="btn btn--ghost" onClick={() => setOpen(false)}>Cancelar</button>
          <button className="btn" onClick={submitReserve} disabled={!clientId}>Confirmar</button>
        </div>
      </Modal>
      {/* lightweight toast */}
      {toast && <div className="toast" onAnimationEnd={() => setToast(undefined)}>{toast}</div>}
    </>
  );
}


