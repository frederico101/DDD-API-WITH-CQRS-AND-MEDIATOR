import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import SearchBar from './SearchBar';
import QuickActions from './QuickActions';

export default function NavBar() {
  const { logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = (path: string) => (location.pathname === path ? 'active' : '');

  const handleSearchResultClick = (result: any) => {
    navigate(result.url);
  };

  return (
    <header className="navbar">
      <div className="navbar__brand">Direcional Imobiliária</div>
      <div className="navbar__search">
        <SearchBar onResultClick={handleSearchResultClick} />
      </div>
      <nav className="navbar__links">
        <Link className={`navlink ${isActive('/') || isActive('/dashboard')}`} to="/">Dashboard</Link>
        <Link className={`navlink ${isActive('/apartments')}`} to="/apartments">Apartamentos</Link>
        <Link className={`navlink ${isActive('/clients')}`} to="/clients">Clientes</Link>
        <Link className={`navlink ${isActive('/reservations')}`} to="/reservations">Reservas</Link>
        <Link className={`navlink ${isActive('/sales')}`} to="/sales">Vendas</Link>
      </nav>
      <div className="navbar__actions">
        <QuickActions />
        <button className="btn btn--ghost" onClick={logout}>Sair</button>
      </div>
    </header>
  );
}


