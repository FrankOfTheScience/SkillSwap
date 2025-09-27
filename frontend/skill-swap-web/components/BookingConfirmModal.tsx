"use client";
import { useState } from "react";
import { Offer, User } from "../types";
import { createCheckoutSession, redirectToCheckout } from "../services/booking";

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

  const handleBooking = async () => {
    if (!offer || !user) return;

    setIsLoading(true);
    setError(null);

    try {
      const checkoutSession = await createCheckoutSession({
        offerId: offer.id,
        userId: user.id, // This will be overridden by backend with token data
      });

      // Redirect to Stripe checkout
      redirectToCheckout(checkoutSession.checkoutUrl);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create booking");
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  // Calculate platform fee (10%)
  const platformFee = offer.price * 0.10;
  const totalAmount = offer.price;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-xl shadow-2xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-gray-900">Confirm Booking</h2>
            <button
              onClick={onClose}
              disabled={isLoading}
              className="text-gray-400 hover:text-gray-600 text-2xl font-bold disabled:opacity-50"
            >
              ×
            </button>
          </div>

          {/* Offer Details */}
          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-4 mb-6">
            <h3 className="font-semibold text-gray-900 mb-2">{offer.title}</h3>
            <p className="text-gray-700 text-sm mb-3 line-clamp-3">{offer.description}</p>
            
            {/* Pricing Breakdown */}
            <div className="border-t pt-3">
              <div className="flex justify-between items-center mb-2">
                <span className="text-gray-700">Service Price:</span>
                <span className="font-semibold">€{offer.price.toFixed(2)}</span>
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
              disabled={isLoading}
              className="flex-1 bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 text-white py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center gap-2"
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
        </div>
      </div>
    </div>
  );
}