"use client";
import { useState } from "react";
import { Offer, User } from "../types";
import { createCheckoutSession, redirectToCheckout } from "../services/booking";
import ModalWrapper from "./ModalWrapper";
import DateTimeSelector from "./DateTimeSelector";

interface BookingConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  offer: Offer;
  user: User;
}

export default function BookingConfirmModal({ 
  isOpen, 
  onClose, 
  offer, 
  user 
}: BookingConfirmModalProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedDateTime, setSelectedDateTime] = useState<string | null>(null);
  const [selectedDuration, setSelectedDuration] = useState<number>(offer.durationInMinutes);
  const [customerNotes, setCustomerNotes] = useState("");
  const [showDateTimeSelector, setShowDateTimeSelector] = useState(false);

  const handleDateTimeSelect = (dateTime: string, durationInMinutes: number) => {
    setSelectedDateTime(dateTime);
    setSelectedDuration(durationInMinutes);
    setShowDateTimeSelector(false);
  };

  const handleBooking = async () => {
    if (!offer || !user || !selectedDateTime) return;

    setIsLoading(true);
    setError(null);

    try {
      const checkoutSession = await createCheckoutSession({
        offerId: offer.id,
        userId: user.id, // This will be overridden by backend with token data
        scheduledDateTime: selectedDateTime,
        durationInMinutes: selectedDuration,
        customerNotes: customerNotes || undefined,
        location: offer.location,
        isOnline: offer.isOnline
      });

      // Redirect to Stripe checkout
      redirectToCheckout(checkoutSession.checkoutUrl);
    } catch (err) {
      console.error('Payment error:', err);
      let errorMessage = "Failed to create booking";
      
      if (err instanceof Error) {
        // Check for specific Stripe configuration errors
        if (err.message.includes("No such customer") || 
            err.message.includes("Invalid API key") ||
            err.message.includes("No API key provided") ||
            err.message.includes("401") ||
            err.message.includes("Unauthorized")) {
          errorMessage = "Payment system configuration error. Please contact support or check Stripe configuration.";
        } else {
          errorMessage = err.message;
        }
      }
      
      setError(errorMessage);
      setIsLoading(false);
    }
  };

  const formatDateTime = (dateTimeString: string) => {
    const date = new Date(dateTimeString);
    return date.toLocaleString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  };

  // Calculate platform fee (10%)
  const platformFee = offer.price * 0.10;
  const totalAmount = offer.price;

  if (showDateTimeSelector) {
    return (
      <ModalWrapper title="Select Date & Time" isOpen={isOpen} onClose={onClose}>
        <DateTimeSelector
          offer={offer}
          selectedDateTime={selectedDateTime}
          onDateTimeSelect={handleDateTimeSelect}
          onClose={() => setShowDateTimeSelector(false)}
          isInModal={true}
        />
      </ModalWrapper>
    );
  }

  return (
    <ModalWrapper title="Confirm Booking" isOpen={isOpen} onClose={onClose}>
        {/* Offer Details */}
        <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-4 mb-6">
          <h3 className="font-semibold text-gray-900 mb-2">{offer.title}</h3>
          <p className="text-gray-700 text-sm mb-3 line-clamp-3">{offer.description}</p>
          
          <div className="flex items-center gap-4 text-sm text-gray-600 mb-3">
            <span className="flex items-center gap-1">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {selectedDuration} minutes
            </span>
            <span className="flex items-center gap-1">
              {offer.isOnline ? (
                <>
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9v-9m0-9v9m0 9c-5 0-9-4-9-9s4-9 9-9" />
                  </svg>
                  Online
                </>
              ) : (
                <>
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  {offer.location || 'In-person'}
                </>
              )}
            </span>
          </div>
          
          {/* Pricing Breakdown */}
          <div className="border-t pt-3">
            <div className="flex justify-between items-center mb-2">
              <span className="text-gray-700">Service Price:</span>
              <span className="font-semibold text-gray-900">€{offer.price.toFixed(2)}</span>
            </div>
            <div className="flex justify-between items-center mb-2 text-sm text-gray-600">
              <span>Platform Fee (10%):</span>
              <span>€{platformFee.toFixed(2)}</span>
            </div>
            <div className="border-t pt-2 flex justify-between items-center">
              <span className="text-lg font-bold text-gray-900">Total:</span>
              <span className="text-lg font-bold text-emerald-600">€{totalAmount.toFixed(2)}</span>
            </div>
          </div>
        </div>

        {/* Date & Time Selection */}
        <div className="bg-gray-50 rounded-lg p-4 mb-6">
          <h4 className="font-semibold text-gray-900 mb-3">Date & Time</h4>
          {selectedDateTime ? (
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-900 font-medium">
                  {formatDateTime(selectedDateTime)}
                </p>
                <p className="text-sm text-gray-600">
                  Duration: {selectedDuration} minutes
                </p>
              </div>
              <button
                onClick={() => setShowDateTimeSelector(true)}
                className="text-blue-600 hover:text-blue-700 text-sm font-medium"
              >
                Change
              </button>
            </div>
          ) : (
            <button
              onClick={() => setShowDateTimeSelector(true)}
              className="w-full p-3 border-2 border-dashed border-gray-300 rounded-lg text-gray-600 hover:border-blue-300 hover:text-blue-600 transition-colors"
            >
              <svg className="w-6 h-6 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              Select Date & Time
            </button>
          )}
        </div>

        {/* Customer Notes */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-900 mb-2">
            Notes for Provider (Optional)
          </label>
          <textarea
            value={customerNotes}
            onChange={(e) => setCustomerNotes(e.target.value)}
            placeholder="Any special requirements or questions..."
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900 placeholder-gray-500"
          />
        </div>

        {/* User Info */}
        <div className="bg-gray-50 rounded-lg p-4 mb-6">
          <h4 className="font-semibold text-gray-900 mb-2">Booking Details</h4>
          <div className="text-sm text-gray-700">
            <p><span className="font-medium">Customer:</span> {user.displayName}</p>
            <p><span className="font-medium">Email:</span> {user.email}</p>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
            <div className="flex items-center">
              <span className="text-red-600 text-xl mr-2">⚠️</span>
              <p className="text-red-700 text-sm">{error}</p>
            </div>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex gap-4">
          <button
            onClick={onClose}
            disabled={isLoading}
            className="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-800 py-3 rounded-lg font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Cancel
          </button>
          <button
            onClick={handleBooking}
            disabled={isLoading || !selectedDateTime}
            className="flex-1 bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 text-white py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center gap-2"
            title={!selectedDateTime ? "Please select a date and time first" : ""}
          >
            {isLoading ? (
              <>
                <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                Processing...
              </>
            ) : (
              <>
                💳 Proceed to Payment
              </>
            )}
          </button>
        </div>

        {/* Payment Security Info */}
        <div className="mt-4 text-center">
          <p className="text-xs text-gray-500">
            🔒 Secure payment powered by Stripe. Your payment information is encrypted and secure.
          </p>
        </div>
    </ModalWrapper>
  );
}