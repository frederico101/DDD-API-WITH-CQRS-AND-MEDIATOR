import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Modal from './Modal';

interface QuickActionsProps {
  className?: string;
}

export default function QuickActions({ className = '' }: QuickActionsProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedAction, setSelectedAction] = useState<string | null>(null);
  const navigate = useNavigate();

  const quickActions = [
    {
      id: 'new-apartment',
      title: 'Novo Apartamento',
      description: 'Adicionar um novo apartamento ao sistema',
      icon: '🏢',
      action: () => navigate('/apartments')
    },
    {
      id: 'new-client',
      title: 'Novo Cliente',
      description: 'Cadastrar um novo cliente',
      icon: '👤',
      action: () => navigate('/clients')
    },
    {
      id: 'new-reservation',
      title: 'Nova Reserva',
      description: 'Criar uma nova reserva',
      icon: '📅',
      action: () => navigate('/reservations')
    },
    {
      id: 'view-dashboard',
      title: 'Dashboard',
      description: 'Ver resumo geral do sistema',
      icon: '📊',
      action: () => navigate('/dashboard')
    }
  ];

  const handleActionClick = (action: typeof quickActions[0]) => {
    action.action();
    setIsOpen(false);
  };

  return (
    <>
      <button
        className={`quick-actions-btn ${className}`}
        onClick={() => setIsOpen(true)}
        title="Ações Rápidas"
      >
        <span className="quick-actions-icon">⚡</span>
        <span className="quick-actions-text">Ações Rápidas</span>
      </button>

      <Modal open={isOpen} title="Ações Rápidas" onClose={() => setIsOpen(false)}>
        <div className="quick-actions-grid">
          {quickActions.map((action) => (
            <button
              key={action.id}
              className="quick-action-item"
              onClick={() => handleActionClick(action)}
            >
              <div className="quick-action-icon">{action.icon}</div>
              <div className="quick-action-content">
                <div className="quick-action-title">{action.title}</div>
                <div className="quick-action-description">{action.description}</div>
              </div>
            </button>
          ))}
        </div>
      </Modal>
    </>
  );
}
