import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function NavBar() {
  const { logout } = useAuth();
  const location = useLocation();

  const isActive = (path: string) => (location.pathname === path ? 'active' : '');

  return (
    <header className="navbar">
      <div className="navbar__brand">Direcional Imobiliária</div>
      <nav className="navbar__links">
        <Link className={`navlink ${isActive('/')}`} to="/">Apartamentos</Link>
      </nav>
      <div className="navbar__actions">
        <button className="btn btn--ghost" onClick={logout}>Sair</button>
      </div>
    </header>
  );
}


