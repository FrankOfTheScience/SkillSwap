export interface Offer {
  id: number;
  title: string;
  description: string;
  price: number;
  createdBy: string;
  createdAt: string;
  updatedAt?: string;
  
  // Availability information
  durationInMinutes: number;
  location?: string;
  isOnline: boolean;
  requirements?: string;
  
  // Availability schedule
  availabilities: OfferAvailability[];
  
  // Offer status
  isActive: boolean;
  deactivatedAt?: string;
  
  // Categories and tags
  category?: string;
  tags: string[];
  
  // Navigation properties
  creator?: User;
}

export interface OfferAvailability {
  id: number;
  offerId: number;
  dayOfWeek: number; // 0 = Sunday, 1 = Monday, etc.
  startTime: string; // HH:mm format
  endTime: string; // HH:mm format
  isAvailable: boolean;
}

export interface User {
  id: string;
  email: string;
  displayName: string;
  role: "User" | "Admin";
  token?: string;
  createdAt: string;
  
  // Profile information
  firstName?: string;
  lastName?: string;
  bio?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  city?: string;
  country?: string;
  profileImageUrl?: string;
  
  // Profile completion tracking
  profileCompletionPercentage: number;
  lastProfileUpdate?: string;
  
  // Professional information
  profession?: string;
  company?: string;
  yearsOfExperience: number;
  skills: string[];
  
  // User preferences
  preferredLanguage: string;
  timeZone?: string;
  emailNotifications: boolean;
  pushNotifications: boolean;
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
  cancelledAt?: string;
  
  // Scheduling information
  scheduledDateTime: string;
  durationInMinutes: number;
  location?: string;
  isOnline: boolean;
  
  // Booking details
  customerNotes?: string;
  providerNotes?: string;
  
  // Meeting information
  meetingUrl?: string;
  meetingId?: string;
  meetingPassword?: string;
  
  // Feedback and rating
  customerRating?: number; // 1-5 stars
  customerFeedback?: string;
  providerRating?: number; // 1-5 stars
  providerFeedback?: string;
  feedbackSubmittedAt?: string;
  
  // Navigation properties
  offer?: Offer;
  user?: User;
}

export interface CreateBookingRequest {
  offerId: number;
  userId: string; // This will be overridden by backend with token data
  scheduledDateTime: string;
  customerNotes?: string;
}

export interface CreateCheckoutSessionRequest {
  offerId: number;
  userId: string; // This will be overridden by backend with token data  
  scheduledDateTime: string;
  customerNotes?: string;
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
  bookingId: number;
}

// User Profile Types
export interface UpdateUserProfileRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  city?: string;
  country?: string;
  profession?: string;
  company?: string;
  yearsOfExperience?: number;
  skills?: string[];
  preferredLanguage?: string;
  timeZone?: string;
  emailNotifications?: boolean;
  pushNotifications?: boolean;
}

export interface ProfileCompletionStatus {
  percentage: number;
  missingFields: string[];
  suggestions: string[];
}

// Offer Management Types
export interface CreateOfferRequest {
  title: string;
  description: string;
  price: number;
  durationInMinutes: number;
  location?: string;
  isOnline: boolean;
  requirements?: string;
  category?: string;
  tags?: string[];
  availabilities: CreateOfferAvailabilityRequest[];
}

export interface CreateOfferAvailabilityRequest {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}

export interface UpdateOfferRequest extends Partial<CreateOfferRequest> {
  isActive?: boolean;
}

// Dashboard Types
export interface UserDashboardStats {
  totalBookings: number;
  completedBookings: number;
  pendingBookings: number;
  cancelledBookings: number;
  totalEarnings: number;
  averageRating: number;
  totalOffers: number;
  activeOffers: number;
}

export interface AdminDashboardStats {
  totalUsers: number;
  totalOffers: number;
  totalBookings: number;
  totalRevenue: number;
  averageRating: number;
  newUsersThisMonth: number;
  bookingsThisMonth: number;
  revenueThisMonth: number;
}

// Paginated response type
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
