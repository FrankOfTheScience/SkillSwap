"use client";

interface ErrorDisplayProps {
  error: string;
  title?: string;
  onRetry?: () => void;
  retryText?: string;
}

export default function ErrorDisplay({ 
  error, 
  title = "Something went wrong", 
  onRetry, 
  retryText = "Try again" 
}: ErrorDisplayProps) {
  return (
    <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
      <div className="text-red-500 text-xl flex-shrink-0">⚠️</div>
      <div className="flex-1">
        <h4 className="font-semibold text-red-800 mb-1">{title}</h4>
        <p className="text-red-700 text-sm mb-3">{error}</p>
        {onRetry && (
          <button
            onClick={onRetry}
            className="text-sm bg-red-100 hover:bg-red-200 text-red-800 px-3 py-1 rounded transition-colors font-medium"
          >
            {retryText}
          </button>
        )}
      </div>
    </div>
  );
}