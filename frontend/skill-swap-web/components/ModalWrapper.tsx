"use client";
import { useRouter } from "next/navigation";
import { ReactNode, useEffect } from "react";

interface ModalWrapperProps {
  children: ReactNode;
  title: string;
  isOpen?: boolean;
  onClose?: () => void;
}

export default function ModalWrapper({ children, title, isOpen = true, onClose }: ModalWrapperProps) {
  const router = useRouter();

  useEffect(() => {
    if (isOpen) {
      // Disable body scrolling when modal is open
      document.body.style.overflow = 'hidden';
    }
    
    return () => {
      // Re-enable body scrolling when modal closes
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      if (onClose) {
        onClose();
      } else {
        router.push("/");
      }
    }
  };

  const handleBackClick = () => {
    if (onClose) {
      onClose();
    } else {
      router.push("/");
    }
  };

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop blur overlay - now transparent */}
      <div 
        className="fixed inset-0 backdrop-blur-md z-40"
        onClick={handleBackdropClick}
      />
      
      {/* Modal container */}
      <div className="fixed inset-0 flex items-center justify-center z-50 p-4 pointer-events-none">
        <div className="relative w-full max-w-2xl max-h-[90vh] overflow-y-auto pointer-events-auto">
          {/* Modal Content */}
          <div className="bg-white rounded-2xl shadow-2xl p-8 border border-gray-200 relative">
            {/* Stylish Back Button - inside the modal content */}
            <button
              onClick={handleBackClick}
              className="absolute top-4 right-4 z-10 w-10 h-10 bg-gray-100 hover:bg-gray-200 rounded-full shadow-md flex items-center justify-center transition-all duration-200 transform hover:scale-110 group"
            >
              <svg 
                className="w-5 h-5 text-gray-600 group-hover:text-gray-800 transition-colors" 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path 
                  strokeLinecap="round" 
                  strokeLinejoin="round" 
                  strokeWidth={2} 
                  d="M6 18L18 6M6 6l12 12" 
                />
              </svg>
            </button>
            
            <div className="text-center mb-6">
              <h1 className="text-2xl font-bold text-gray-800">{title}</h1>
            </div>
            {children}
          </div>
        </div>
      </div>
    </>
  );
}