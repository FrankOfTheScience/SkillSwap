import { AxiosError } from 'axios';

export interface ApiErrorResponse {
  message: string;
  type: string;
}

export const getErrorMessage = (error: unknown): string => {
  // If it's an axios error with a response
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as AxiosError<ApiErrorResponse>;
    
    // If the backend returned a structured error message
    if (axiosError.response?.data?.message) {
      return axiosError.response.data.message;
    }
    
    // If there's a response status, provide appropriate message
    if (axiosError.response?.status) {
      switch (axiosError.response.status) {
        case 400:
          return "Invalid request. Please check your input and try again.";
        case 401:
          return "You are not authorized to perform this action.";
        case 403:
          return "Access denied. You don't have permission for this action.";
        case 404:
          return "The requested resource was not found.";
        case 409:
          return "This resource already exists. Please use a different value.";
        case 408:
          return "The request timed out. Please try again.";
        case 429:
          return "Too many requests. Please wait a moment before trying again.";
        case 500:
          return "Something went wrong on our end. Please try again later.";
        case 502:
        case 503:
        case 504:
          return "The service is temporarily unavailable. Please try again later.";
        default:
          return "An unexpected error occurred. Please try again.";
      }
    }
  }
  
  // If it's a network error
  if (error && typeof error === 'object' && 'message' in error) {
    const message = (error as Error).message;
    if (message.includes('Network Error') || message.includes('timeout')) {
      return "Unable to connect to the server. Please check your internet connection and try again.";
    }
  }
  
  // Default fallback message
  return "Something went wrong. Please try again.";
};

export const showErrorToast = (error: unknown, defaultMessage?: string): string => {
  const message = defaultMessage || getErrorMessage(error);
  
  // In a real app, you might want to use a toast library here
  // For now, we'll just console.error and return the message
  console.error('Error occurred:', error);
  
  return message;
};