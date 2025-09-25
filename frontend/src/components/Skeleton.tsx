interface SkeletonProps {
  width?: string | number;
  height?: string | number;
  className?: string;
  variant?: 'text' | 'rectangular' | 'circular';
}

export default function Skeleton({ 
  width = '100%', 
  height = '1rem', 
  className = '', 
  variant = 'rectangular' 
}: SkeletonProps) {
  const baseClasses = 'skeleton';
  const variantClasses = {
    text: 'skeleton--text',
    rectangular: 'skeleton--rectangular',
    circular: 'skeleton--circular'
  };

  return (
    <div
      className={`${baseClasses} ${variantClasses[variant]} ${className}`}
      style={{ width, height }}
    />
  );
}

// Pre-built skeleton components for common use cases
export function TableSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="table-skeleton">
      <div className="table-skeleton-header">
        <Skeleton width="60px" height="45px" />
        <Skeleton width="20%" height="1.5rem" />
        <Skeleton width="15%" height="1.5rem" />
        <Skeleton width="15%" height="1.5rem" />
        <Skeleton width="15%" height="1.5rem" />
        <Skeleton width="15%" height="1.5rem" />
        <Skeleton width="20%" height="1.5rem" />
      </div>
      {Array.from({ length: rows }).map((_, index) => (
        <div key={index} className="table-skeleton-row">
          <Skeleton width="60px" height="45px" />
          <Skeleton width="20%" height="1rem" />
          <Skeleton width="15%" height="1rem" />
          <Skeleton width="15%" height="1rem" />
          <Skeleton width="15%" height="1rem" />
          <Skeleton width="15%" height="1rem" />
          <Skeleton width="20%" height="1rem" />
        </div>
      ))}
    </div>
  );
}

export function CardSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="card-skeleton-grid">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="card-skeleton">
          <Skeleton width="100%" height="120px" />
          <div className="card-skeleton-content">
            <Skeleton width="80%" height="1.25rem" />
            <Skeleton width="60%" height="1rem" />
            <Skeleton width="40%" height="1rem" />
          </div>
        </div>
      ))}
    </div>
  );
}

export function MetricSkeleton({ count = 8 }: { count?: number }) {
  return (
    <div className="metrics-skeleton-grid">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="metric-skeleton">
          <Skeleton width="40px" height="40px" variant="circular" />
          <div className="metric-skeleton-content">
            <Skeleton width="60px" height="1.5rem" />
            <Skeleton width="80px" height="1rem" />
          </div>
        </div>
      ))}
    </div>
  );
}
