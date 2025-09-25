import { useState, useEffect } from 'react';
import { api } from '../api/client';
import NavBar from '../components/NavBar';
import { MetricSkeleton } from '../components/Skeleton';

type Apartment = { id: string; code: string; block: string; floor: number; number: number; price: number; status: number };
type Client = { id: string; name: string; email: string; phone: string };
type Reservation = { id: string; clientId: string; clientName: string; apartmentId: string; apartmentCode: string; expiresAtUtc?: string; confirmedAsSale: boolean };
type Sale = { id: string; clientId: string; clientName: string; apartmentId: string; apartmentCode: string; downPayment: number; totalPrice: number; createdAtUtc: string };
type Paged<T> = { total: number; items: T[] };

interface DashboardStats {
  totalApartments: number;
  availableApartments: number;
  reservedApartments: number;
  soldApartments: number;
  totalClients: number;
  activeReservations: number;
  totalSales: number;
  totalRevenue: number;
  avgPrice: number;
}

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats>({
    totalApartments: 0,
    availableApartments: 0,
    reservedApartments: 0,
    soldApartments: 0,
    totalClients: 0,
    activeReservations: 0,
    totalSales: 0,
    totalRevenue: 0,
    avgPrice: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();
  const [recentReservations, setRecentReservations] = useState<Reservation[]>([]);
  const [recentSales, setRecentSales] = useState<Sale[]>([]);

  useEffect(() => {
    (async () => {
      try {
        const [apartmentsRes, clientsRes, reservationsRes, salesRes] = await Promise.all([
          api.get<Paged<Apartment>>('/apartments', { params: { page: 1, pageSize: 1000 } }),
          api.get<Paged<Client>>('/clients', { params: { page: 1, pageSize: 1000 } }),
          api.get<Paged<Reservation>>('/reservations', { params: { page: 1, pageSize: 1000 } }),
          api.get<Paged<Sale>>('/sales', { params: { page: 1, pageSize: 1000 } })
        ]);

        const apartments = apartmentsRes.data.items;
        const clients = clientsRes.data.items;
        const reservations = reservationsRes.data.items;
        const sales = salesRes.data.items;

        // Calculate stats
        const totalApartments = apartments.length;
        const availableApartments = apartments.filter(a => a.status === 0).length;
        const reservedApartments = apartments.filter(a => a.status === 1).length;
        const soldApartments = apartments.filter(a => a.status === 2).length;
        const totalClients = clients.length;
        const activeReservations = reservations.filter(r => !r.confirmedAsSale).length;
        const totalSales = sales.length;
        const totalRevenue = sales.reduce((sum, sale) => sum + sale.totalPrice, 0);
        const avgPrice = apartments.length > 0 ? apartments.reduce((sum, apt) => sum + apt.price, 0) / apartments.length : 0;

        setStats({
          totalApartments,
          availableApartments,
          reservedApartments,
          soldApartments,
          totalClients,
          activeReservations,
          totalSales,
          totalRevenue,
          avgPrice
        });

        // Get recent data
        setRecentReservations(reservations.slice(0, 5));
        setRecentSales(sales.slice(0, 5));

      } catch (e: any) {
        setError(e?.response?.data?.detail || 'Erro ao carregar dados');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  const getStatusColor = (status: number) => {
    switch (status) {
      case 0: return '#059669'; // Available - green
      case 1: return '#d97706'; // Reserved - orange
      case 2: return '#dc2626'; // Sold - red
      default: return '#6b7280'; // Unknown - gray
    }
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 0: return 'Disponível';
      case 1: return 'Reservado';
      case 2: return 'Vendido';
      default: return 'Desconhecido';
    }
  };

  if (loading) {
    return (
      <>
        <NavBar />
        <div className="container">
          <div className="panel">
            <h2>Dashboard</h2>
            <MetricSkeleton count={8} />
          </div>
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <NavBar />
        <div className="container">
          <div className="panel">
            <h2>Dashboard</h2>
            <p style={{ color: 'salmon' }}>{error}</p>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <NavBar />
      <div className="container">
        <div className="panel">
          <h2 style={{ marginTop: 0 }}>Dashboard</h2>
          
          {/* Key Metrics */}
          <div className="metrics-grid">
            <div className="metric-card">
              <div className="metric-icon">🏢</div>
              <div className="metric-content">
                <div className="metric-value">{stats.totalApartments}</div>
                <div className="metric-label">Total Apartamentos</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">✅</div>
              <div className="metric-content">
                <div className="metric-value">{stats.availableApartments}</div>
                <div className="metric-label">Disponíveis</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">⏰</div>
              <div className="metric-content">
                <div className="metric-value">{stats.reservedApartments}</div>
                <div className="metric-label">Reservados</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">💰</div>
              <div className="metric-content">
                <div className="metric-value">{stats.soldApartments}</div>
                <div className="metric-label">Vendidos</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">👥</div>
              <div className="metric-content">
                <div className="metric-value">{stats.totalClients}</div>
                <div className="metric-label">Clientes</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">📋</div>
              <div className="metric-content">
                <div className="metric-value">{stats.activeReservations}</div>
                <div className="metric-label">Reservas Ativas</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">🎯</div>
              <div className="metric-content">
                <div className="metric-value">{stats.totalSales}</div>
                <div className="metric-label">Vendas</div>
              </div>
            </div>
            
            <div className="metric-card">
              <div className="metric-icon">💵</div>
              <div className="metric-content">
                <div className="metric-value">{formatCurrency(stats.totalRevenue)}</div>
                <div className="metric-label">Receita Total</div>
              </div>
            </div>
          </div>

          {/* Status Distribution */}
          <div className="dashboard-section">
            <h3>Distribuição por Status</h3>
            <div className="status-chart">
              <div className="status-bar">
                <div 
                  className="status-segment" 
                  style={{ 
                    width: `${(stats.availableApartments / stats.totalApartments) * 100}%`,
                    backgroundColor: getStatusColor(0)
                  }}
                ></div>
                <div 
                  className="status-segment" 
                  style={{ 
                    width: `${(stats.reservedApartments / stats.totalApartments) * 100}%`,
                    backgroundColor: getStatusColor(1)
                  }}
                ></div>
                <div 
                  className="status-segment" 
                  style={{ 
                    width: `${(stats.soldApartments / stats.totalApartments) * 100}%`,
                    backgroundColor: getStatusColor(2)
                  }}
                ></div>
              </div>
              <div className="status-legend">
                <div className="legend-item">
                  <div className="legend-color" style={{ backgroundColor: getStatusColor(0) }}></div>
                  <span>Disponível ({stats.availableApartments})</span>
                </div>
                <div className="legend-item">
                  <div className="legend-color" style={{ backgroundColor: getStatusColor(1) }}></div>
                  <span>Reservado ({stats.reservedApartments})</span>
                </div>
                <div className="legend-item">
                  <div className="legend-color" style={{ backgroundColor: getStatusColor(2) }}></div>
                  <span>Vendido ({stats.soldApartments})</span>
                </div>
              </div>
            </div>
          </div>

          {/* Recent Activity */}
          <div className="dashboard-section">
            <h3>Reservas Recentes</h3>
            {recentReservations.length > 0 ? (
              <div className="activity-list">
                {recentReservations.map(reservation => (
                  <div key={reservation.id} className="activity-item">
                    <div className="activity-icon">📅</div>
                    <div className="activity-content">
                      <div className="activity-title">
                        {reservation.apartmentCode} reservado por {reservation.clientName}
                      </div>
                      <div className="activity-time">
                        Expira em: {reservation.expiresAtUtc ? new Date(reservation.expiresAtUtc).toLocaleString('pt-BR') : 'Não definido'}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p style={{ color: '#6b7280', fontStyle: 'italic' }}>Nenhuma reserva recente</p>
            )}
          </div>

          {/* Recent Sales */}
          <div className="dashboard-section">
            <h3>Vendas Recentes</h3>
            {recentSales.length > 0 ? (
              <div className="activity-list">
                {recentSales.map(sale => (
                  <div key={sale.id} className="activity-item">
                    <div className="activity-icon">💰</div>
                    <div className="activity-content">
                      <div className="activity-title">
                        {sale.apartmentCode} vendido para {sale.clientName}
                      </div>
                      <div className="activity-time">
                        Valor: {formatCurrency(sale.totalPrice)} • Entrada: {formatCurrency(sale.downPayment)}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p style={{ color: '#6b7280', fontStyle: 'italic' }}>Nenhuma venda recente</p>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
