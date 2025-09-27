export interface Offer {
  id: number;
  title: string;
  description: string;
  price: number;
  createdBy: string;
}

export interface User {
  id: string;  // This should be string because JWT tokens contain string values
  email: string;
  displayName: string;
  role: "User" | "Admin";
  token?: string;
}

// Booking-related types
export enum BookingStatus {
  Pending = "Pending",
  Completed = "Completed", 
  Cancelled = "Cancelled",
  Refunded = "Refunded"
}

export interface Booking {
  id: number;
  offerId: number;
  userId: string;
  status: BookingStatus;
  amount: number;
  commissionAmount: number;
  stripeCheckoutSessionId?: string;
  stripePaymentIntentId?: string;
  createdAt: string;
  completedAt?: string;
  
  // Navigation properties (may be null depending on API response)
  offer?: Offer;
  user?: User;
}

export interface CreateBookingRequest {
  offerId: number;
  userId: string; // This will be overridden by backend with token data
}

export interface CreateCheckoutSessionRequest {
  offerId: number;
  userId: string; // This will be overridden by backend with token data  
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
  bookingId: number;
}

// Paginated response type
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
