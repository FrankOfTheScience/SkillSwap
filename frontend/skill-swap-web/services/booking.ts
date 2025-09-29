import api from "./api";
import { 
  CreateCheckoutSessionRequest, 
  CreateCheckoutSessionResponse, 
  Booking,
  PaginatedResponse 
} from "../types";
import { getErrorMessage } from "../utils/errorHandler";

/**
 * Creates a Stripe checkout session for booking an offer
 * @param request - The checkout session request data
 * @returns Promise with checkout session response containing URL and booking ID
 */
export const createCheckoutSession = async (
  request: CreateCheckoutSessionRequest
): Promise<CreateCheckoutSessionResponse> => {
  try {
    const response = await api.post<CreateCheckoutSessionResponse>("/api/checkout/session", request);
    return response.data;
  } catch (error) {
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
};

/**
 * Gets the current user's bookings with optional pagination
 * @param page - Page number (default: 1)
 * @param pageSize - Number of items per page (default: 10)
 * @returns Promise with paginated booking results
 */
export const getUserBookings = async (
  page: number = 1,
  pageSize: number = 10
): Promise<PaginatedResponse<Booking>> => {
  try {
    const response = await api.get<PaginatedResponse<Booking>>("/api/bookings", {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
};

/**
 * Gets a specific booking by ID
 * @param bookingId - The ID of the booking to retrieve
 * @returns Promise with booking details
 */
export const getBooking = async (bookingId: number): Promise<Booking> => {
  try {
    const response = await api.get<Booking>(`/api/bookings/${bookingId}`);
    return response.data;
  } catch (error) {
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
};

/**
 * Cancels a booking (if allowed by business rules)
 * @param bookingId - The ID of the booking to cancel
 * @returns Promise with success confirmation
 */
export const cancelBooking = async (bookingId: number): Promise<void> => {
  try {
    await api.patch(`/api/bookings/${bookingId}/cancel`);
  } catch (error) {
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
};

/**
 * Redirects to Stripe checkout for payment processing
 * @param checkoutUrl - The Stripe checkout URL
 */
export const redirectToCheckout = (checkoutUrl: string): void => {
  window.location.href = checkoutUrl;
};