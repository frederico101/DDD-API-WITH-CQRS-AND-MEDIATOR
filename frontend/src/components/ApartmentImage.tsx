import { useState } from 'react';

interface ApartmentImageProps {
  apartmentCode: string;
  className?: string;
  size?: 'small' | 'medium' | 'large';
}

export default function ApartmentImage({ 
  apartmentCode, 
  className = '', 
  size = 'medium'
}: ApartmentImageProps) {
  const [imageError, setImageError] = useState(false);
  const [imageLoading, setImageLoading] = useState(true);

  // Generate a placeholder image URL based on apartment code
  const getPlaceholderImage = (code: string) => {
    // Use a placeholder service or generate a simple colored background
    const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'];
    const colorIndex = code.charCodeAt(0) % colors.length;
    const color = colors[colorIndex];
    
    // Create a simple SVG placeholder
    const svg = `
      <svg width="200" height="150" xmlns="http://www.w3.org/2000/svg">
        <rect width="200" height="150" fill="${color}"/>
        <text x="100" y="75" font-family="Arial, sans-serif" font-size="24" fill="white" text-anchor="middle" dominant-baseline="middle">${code}</text>
      </svg>
    `;
    
    return `data:image/svg+xml;base64,${btoa(svg)}`;
  };

  const sizeClasses = {
    small: 'apartment-image--small',
    medium: 'apartment-image--medium',
    large: 'apartment-image--large'
  };

  const handleImageLoad = () => {
    setImageLoading(false);
  };

  const handleImageError = () => {
    setImageError(true);
    setImageLoading(false);
  };

  return (
    <div className={`apartment-image ${sizeClasses[size]} ${className}`}>
      {imageLoading && (
        <div className="apartment-image-loading">
          <div className="apartment-image-spinner"></div>
        </div>
      )}
      
      {!imageError ? (
        <img
          src={getPlaceholderImage(apartmentCode)}
          alt={`Apartamento ${apartmentCode}`}
          onLoad={handleImageLoad}
          onError={handleImageError}
          className="apartment-image-img"
          style={{ display: imageLoading ? 'none' : 'block' }}
        />
      ) : (
        <div className="apartment-image-error">
          <div className="apartment-image-error-icon">🏢</div>
          <div className="apartment-image-error-text">{apartmentCode}</div>
        </div>
      )}
    </div>
  );
}

// Gallery component for multiple apartment images
interface ApartmentGalleryProps {
  apartmentCode: string;
  images?: string[];
  className?: string;
}

// Helper function for generating placeholder images
const getPlaceholderImage = (code: string) => {
  const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'];
  const colorIndex = code.charCodeAt(0) % colors.length;
  const color = colors[colorIndex];
  
  const svg = `
    <svg width="400" height="300" xmlns="http://www.w3.org/2000/svg">
      <rect width="400" height="300" fill="${color}"/>
      <text x="200" y="150" font-family="Arial, sans-serif" font-size="48" fill="white" text-anchor="middle" dominant-baseline="middle">${code}</text>
    </svg>
  `;
  
  return `data:image/svg+xml;base64,${btoa(svg)}`;
};

export function ApartmentGallery({ apartmentCode, images = [], className = '' }: ApartmentGalleryProps) {
  const [selectedImage, setSelectedImage] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // If no images provided, use placeholder
  const displayImages = images.length > 0 ? images : [getPlaceholderImage(apartmentCode)];

  return (
    <div className={`apartment-gallery ${className}`}>
      <div className="apartment-gallery-main">
        <img
          src={displayImages[selectedImage]}
          alt={`Apartamento ${apartmentCode} - Imagem ${selectedImage + 1}`}
          className="apartment-gallery-main-img"
          onClick={() => setIsModalOpen(true)}
        />
        {displayImages.length > 1 && (
          <div className="apartment-gallery-nav">
            <button
              className="apartment-gallery-nav-btn"
              onClick={() => setSelectedImage(Math.max(0, selectedImage - 1))}
              disabled={selectedImage === 0}
            >
              ←
            </button>
            <span className="apartment-gallery-counter">
              {selectedImage + 1} / {displayImages.length}
            </span>
            <button
              className="apartment-gallery-nav-btn"
              onClick={() => setSelectedImage(Math.min(displayImages.length - 1, selectedImage + 1))}
              disabled={selectedImage === displayImages.length - 1}
            >
              →
            </button>
          </div>
        )}
      </div>
      
      {displayImages.length > 1 && (
        <div className="apartment-gallery-thumbnails">
          {displayImages.map((image, index) => (
            <img
              key={index}
              src={image}
              alt={`Thumbnail ${index + 1}`}
              className={`apartment-gallery-thumbnail ${selectedImage === index ? 'active' : ''}`}
              onClick={() => setSelectedImage(index)}
            />
          ))}
        </div>
      )}

      {/* Modal for full-size view */}
      {isModalOpen && (
        <div className="apartment-gallery-modal" onClick={() => setIsModalOpen(false)}>
          <div className="apartment-gallery-modal-content">
            <img
              src={displayImages[selectedImage]}
              alt={`Apartamento ${apartmentCode} - Imagem ${selectedImage + 1}`}
              className="apartment-gallery-modal-img"
            />
            <button
              className="apartment-gallery-modal-close"
              onClick={() => setIsModalOpen(false)}
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
