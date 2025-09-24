import { useEffect, useState } from 'react';

export default function Toast({ message }: { message?: string }) {
  const [show, setShow] = useState(!!message);
  useEffect(() => { setShow(!!message); if (message) { const t = setTimeout(()=>setShow(false), 2500); return () => clearTimeout(t); } }, [message]);
  if (!show || !message) return null;
  return <div className="toast">{message}</div>;
}


