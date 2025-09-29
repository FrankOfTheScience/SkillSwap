"use client";
import ModalWrapper from "./ModalWrapper";

interface BookingSuccessModalProps {
  isOpen: boolean;
  onClose: () => void;
  onViewMyBookings?: () => void;
}

export default function BookingSuccessModal({ isOpen, onClose, onViewMyBookings }: BookingSuccessModalProps) {
  const handleViewMyBookings = () => {
    if (onViewMyBookings) {
      onViewMyBookings();
    }
    onClose();
  };

  const handleBackToOffers = () => {
    onClose();
  };

  return (
    <ModalWrapper title="Booking Confirmed!" isOpen={isOpen} onClose={onClose}>
      <div className="text-center py-6">
        <div className="text-6xl mb-6">🎉</div>
        <h2 className="text-2xl font-bold text-gray-800 mb-4">Payment Successful!</h2>
        <p className="text-gray-600 mb-8">
          Your booking has been confirmed and the service provider will be notified.
        </p>

        {/* Next Steps */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="font-semibold text-gray-800 mb-2">What happens next?</h3>
          <ul className="text-sm text-gray-700 space-y-1">
            <li>• The service provider will be notified of your booking</li>
            <li>• You will receive a confirmation email shortly</li>
            <li>• The provider will contact you to schedule the service</li>
            <li>• You can view all your bookings in your dashboard</li>
          </ul>
        </div>

        {/* Security Notice */}
        <div className="bg-green-50 border border-green-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="font-semibold text-gray-800 mb-2">🔒 Secure Payment Complete</h3>
          <p className="text-sm text-gray-700">
            Your payment has been processed securely. For security reasons, payment details are not displayed here.
            You can view your booking details in your dashboard.
          </p>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-4 justify-center">
          <button
            onClick={handleViewMyBookings}
            className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-600 hover:to-emerald-700 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center gap-2"
          >
            📋 View My Bookings
          </button>
          <button
            onClick={handleBackToOffers}
            className="bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
          >
            Browse More Offers
          </button>
        </div>
      </div>
    </ModalWrapper>
  );
}