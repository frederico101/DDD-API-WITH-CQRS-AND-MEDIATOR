import { useState, useEffect, useRef } from 'react';
import { api } from '../api/client';

interface SearchResult {
  type: 'apartment' | 'client' | 'reservation' | 'sale';
  id: string;
  title: string;
  subtitle: string;
  url: string;
}

interface SearchBarProps {
  onResultClick?: (result: SearchResult) => void;
}

export default function SearchBar({ onResultClick }: SearchBarProps) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (query.length < 2) {
      setResults([]);
      setIsOpen(false);
      return;
    }

    const searchData = async () => {
      setLoading(true);
      try {
        const [apartmentsRes, clientsRes, reservationsRes, salesRes] = await Promise.all([
          api.get('/apartments', { params: { page: 1, pageSize: 50 } }),
          api.get('/clients', { params: { page: 1, pageSize: 50 } }),
          api.get('/reservations', { params: { page: 1, pageSize: 50 } }),
          api.get('/sales', { params: { page: 1, pageSize: 50 } })
        ]);

        const searchResults: SearchResult[] = [];

        // Search apartments
        apartmentsRes.data.items.forEach((apt: any) => {
          if (apt.code.toLowerCase().includes(query.toLowerCase()) ||
              apt.block.toLowerCase().includes(query.toLowerCase())) {
            searchResults.push({
              type: 'apartment',
              id: apt.id,
              title: `Apartamento ${apt.code}`,
              subtitle: `Bloco ${apt.block}, ${apt.floor}º andar - ${apt.number}º`,
              url: '/apartments'
            });
          }
        });

        // Search clients
        clientsRes.data.items.forEach((client: any) => {
          if (client.name.toLowerCase().includes(query.toLowerCase()) ||
              client.email.toLowerCase().includes(query.toLowerCase())) {
            searchResults.push({
              type: 'client',
              id: client.id,
              title: client.name,
              subtitle: client.email,
              url: '/clients'
            });
          }
        });

        // Search reservations
        reservationsRes.data.items.forEach((reservation: any) => {
          if (reservation.apartmentCode.toLowerCase().includes(query.toLowerCase()) ||
              reservation.clientName.toLowerCase().includes(query.toLowerCase())) {
            searchResults.push({
              type: 'reservation',
              id: reservation.id,
              title: `Reserva ${reservation.apartmentCode}`,
              subtitle: `Cliente: ${reservation.clientName}`,
              url: '/reservations'
            });
          }
        });

        // Search sales
        salesRes.data.items.forEach((sale: any) => {
          if (sale.apartmentCode.toLowerCase().includes(query.toLowerCase()) ||
              sale.clientName.toLowerCase().includes(query.toLowerCase())) {
            searchResults.push({
              type: 'sale',
              id: sale.id,
              title: `Venda ${sale.apartmentCode}`,
              subtitle: `Cliente: ${sale.clientName}`,
              url: '/sales'
            });
          }
        });

        setResults(searchResults.slice(0, 8)); // Limit to 8 results
        setIsOpen(true);
      } catch (error) {
        console.error('Search error:', error);
        setResults([]);
      } finally {
        setLoading(false);
      }
    };

    const timeoutId = setTimeout(searchData, 300); // Debounce search
    return () => clearTimeout(timeoutId);
  }, [query]);

  const handleResultClick = (result: SearchResult) => {
    setIsOpen(false);
    setQuery('');
    if (onResultClick) {
      onResultClick(result);
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'apartment': return '🏢';
      case 'client': return '👤';
      case 'reservation': return '📅';
      case 'sale': return '💰';
      default: return '🔍';
    }
  };

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'apartment': return 'Apartamento';
      case 'client': return 'Cliente';
      case 'reservation': return 'Reserva';
      case 'sale': return 'Venda';
      default: return 'Resultado';
    }
  };

  return (
    <div className="search-container" ref={searchRef}>
      <div className="search-input-wrapper">
        <input
          type="text"
          placeholder="Buscar apartamentos, clientes, reservas..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          className="search-input"
          onFocus={() => query.length >= 2 && setIsOpen(true)}
        />
        <div className="search-icon">🔍</div>
      </div>

      {isOpen && (
        <div className="search-results">
          {loading ? (
            <div className="search-loading">
              <div className="search-spinner"></div>
              <span>Buscando...</span>
            </div>
          ) : results.length > 0 ? (
            <>
              <div className="search-results-header">
                <span>{results.length} resultado(s) encontrado(s)</span>
              </div>
              {results.map((result) => (
                <div
                  key={`${result.type}-${result.id}`}
                  className="search-result-item"
                  onClick={() => handleResultClick(result)}
                >
                  <div className="search-result-icon">
                    {getTypeIcon(result.type)}
                  </div>
                  <div className="search-result-content">
                    <div className="search-result-title">{result.title}</div>
                    <div className="search-result-subtitle">{result.subtitle}</div>
                  </div>
                  <div className="search-result-type">
                    {getTypeLabel(result.type)}
                  </div>
                </div>
              ))}
            </>
          ) : query.length >= 2 ? (
            <div className="search-no-results">
              <span>Nenhum resultado encontrado para "{query}"</span>
            </div>
          ) : null}
        </div>
      )}
    </div>
  );
}
