type Props = {
  open: boolean;
  title?: string;
  onClose: () => void;
  children: React.ReactNode;
};

export default function Modal({ open, title, onClose, children }: Props) {
  if (!open) return null;
  return (
    <div className="modal__backdrop" onClick={onClose}>
      <div className="modal__content" onClick={(e) => e.stopPropagation()}>
        <div className="modal__header">
          <h3 style={{ margin: 0 }}>{title}</h3>
          <button className="btn btn--ghost" onClick={onClose}>Fechar</button>
        </div>
        <div className="modal__body">{children}</div>
      </div>
    </div>
  );
}


