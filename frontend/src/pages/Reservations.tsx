import { useEffect, useState } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';
import Modal from '../components/Modal';

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
  const [saleModalOpen, setSaleModalOpen] = useState(false);
  const [selectedReservation, setSelectedReservation] = useState<Reservation | null>(null);
  const [downPayment, setDownPayment] = useState('');
  const [totalPrice, setTotalPrice] = useState('');
  const [toast, setToast] = useState<string | undefined>();

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

  const openSaleModal = (reservation: Reservation) => {
    setSelectedReservation(reservation);
    setDownPayment('');
    setTotalPrice('');
    setSaleModalOpen(true);
  };

  const confirmSale = async () => {
    if (!selectedReservation || !downPayment || !totalPrice) return;
    
    try {
      await api.post('/sales', {
        clientId: selectedReservation.clientId,
        apartmentId: selectedReservation.apartmentId,
        reservationId: selectedReservation.id,
        downPayment: parseFloat(downPayment),
        totalPrice: parseFloat(totalPrice)
      });
      
      setSaleModalOpen(false);
      setToast('Venda confirmada com sucesso!');
      
      // Refresh reservations data
      const { data } = await api.get<Paged<Reservation>>('/reservations', { params: { page: 1, pageSize: 20 } });
      setData(data);
    } catch (e: any) {
      setToast(e?.response?.data?.detail || 'Erro ao confirmar venda');
    }
  };

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
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map(r => (
                  <tr key={r.id}>
                    <td>{r.apartmentCode}</td>
                    <td>{r.clientName}</td>
                    <td>{r.expiresAtUtc ? new Date(r.expiresAtUtc).toLocaleString('pt-BR') : '-'}</td>
                    <td>{r.confirmedAsSale ? 'Confirmada (Venda)' : 'Reservada'}</td>
                    <td>
                      {!r.confirmedAsSale && (
                        <button 
                          className="btn btn--success" 
                          onClick={() => openSaleModal(r)}
                        >
                          Finalizar Venda
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      <Modal open={saleModalOpen} title="Finalizar Venda" onClose={() => setSaleModalOpen(false)}>
        <p>
          Apartamento: <strong>{selectedReservation?.apartmentCode}</strong>
        </p>
        <p>
          Cliente: <strong>{selectedReservation?.clientName}</strong>
        </p>
        
        <label>Valor Total (R$)</label>
        <input 
          type="number" 
          step="0.01" 
          min="0"
          style={{ width: '100%', marginBottom: 12 }} 
          value={totalPrice} 
          onChange={(e) => setTotalPrice(e.target.value)}
          placeholder="Ex: 420000.00"
        />
        
        <label>Entrada (R$)</label>
        <input 
          type="number" 
          step="0.01" 
          min="0"
          style={{ width: '100%', marginBottom: 12 }} 
          value={downPayment} 
          onChange={(e) => setDownPayment(e.target.value)}
          placeholder="Ex: 50000.00"
        />
        
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button className="btn btn--ghost" onClick={() => setSaleModalOpen(false)}>Cancelar</button>
          <button 
            className="btn btn--success" 
            onClick={confirmSale} 
            disabled={!totalPrice || !downPayment || parseFloat(totalPrice) <= 0 || parseFloat(downPayment) < 0}
          >
            Confirmar Venda
          </button>
        </div>
      </Modal>

      {/* Toast notification */}
      {toast && <div className="toast" onAnimationEnd={() => setToast(undefined)}>{toast}</div>}
    </>
  );
}


