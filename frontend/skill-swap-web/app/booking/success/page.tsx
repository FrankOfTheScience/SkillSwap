"use client";
import { useEffect, useState, Suspense } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { getBooking } from "../../../services/booking";
import { Booking } from "../../../types";
import BookingCard from "../../../components/BookingCard";
import ModalWrapper from "../../../components/ModalWrapper";

function BookingSuccessContent() {
  const [booking, setBooking] = useState<Booking | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const searchParams = useSearchParams();
  const router = useRouter();
  
  const sessionId = searchParams.get("session_id");
  const bookingId = searchParams.get("booking_id");

  useEffect(() => {
    const fetchBookingDetails = async () => {
      if (!bookingId) {
        setError("No booking ID provided");
        setLoading(false);
        return;
      }

      try {
        const bookingData = await getBooking(parseInt(bookingId));
        setBooking(bookingData);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load booking details");
      } finally {
        setLoading(false);
      }
    };

    fetchBookingDetails();
  }, [bookingId]);

  const handleBackToOffers = () => {
    router.push("/");
  };

  const handleViewMyBookings = () => {
    router.push("/my-bookings");
  };

  if (loading) {
    return (
      <ModalWrapper title="Processing Payment">
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-green-600 mx-auto mb-6"></div>
          <h2 className="text-xl font-bold text-gray-800 mb-2">Confirming your booking...</h2>
          <p className="text-gray-600">Please wait while we process your payment.</p>
        </div>
      </ModalWrapper>
    );
  }

  if (error || !booking) {
    return (
      <ModalWrapper title="Booking Error">
        <div className="text-center py-12">
          <div className="text-6xl mb-6">❌</div>
          <h2 className="text-xl font-bold text-gray-800 mb-4">Something went wrong</h2>
          <p className="text-gray-600 mb-8">{error || "Unable to load booking details"}</p>
          <div className="flex gap-4 justify-center">
            <button
              onClick={handleBackToOffers}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold transition-colors"
            >
              Back to Offers
            </button>
            <button
              onClick={handleViewMyBookings}
              className="bg-gray-600 hover:bg-gray-700 text-white px-6 py-3 rounded-lg font-semibold transition-colors"
            >
              My Bookings
            </button>
          </div>
        </div>
      </ModalWrapper>
    );
  }

  return (
    <ModalWrapper title="Booking Confirmed!">
      <div className="text-center py-6">
        <div className="text-6xl mb-6">🎉</div>
        <h2 className="text-2xl font-bold text-gray-800 mb-4">Payment Successful!</h2>
        <p className="text-gray-600 mb-8">
          Your booking has been confirmed and the service provider will be notified.
        </p>

        {/* Booking Details */}
        <div className="max-w-md mx-auto mb-8">
          <BookingCard booking={booking} showActions={false} />
        </div>

        {/* Session Information */}
        {sessionId && (
          <div className="bg-gray-50 rounded-lg p-4 mb-8 text-left">
            <h3 className="font-semibold text-gray-900 mb-2">Payment Details</h3>
            <p className="text-sm text-gray-600">
              <span className="font-medium">Session ID:</span> {sessionId}
            </p>
            <p className="text-xs text-gray-500 mt-2">
              Keep this information for your records.
            </p>
          </div>
        )}

        {/* Next Steps */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="font-semibold text-blue-900 mb-2">What happens next?</h3>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• The service provider will be notified of your booking</li>
            <li>• You will receive a confirmation email shortly</li>
            <li>• The provider will contact you to schedule the service</li>
            <li>• You can view all your bookings in your dashboard</li>
          </ul>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-4 justify-center">
          <button
            onClick={handleViewMyBookings}
            className="bg-emerald-600 hover:bg-emerald-700 text-white px-8 py-3 rounded-lg font-semibold transition-colors flex items-center gap-2"
          >
            📋 View My Bookings
          </button>
          <button
            onClick={handleBackToOffers}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold transition-colors"
          >
            Browse More Offers
          </button>
        </div>
      </div>
    </ModalWrapper>
  );
}

export default function BookingSuccessPage() {
  return (
    <Suspense fallback={
      <ModalWrapper title="Loading">
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading booking details...</p>
        </div>
      </ModalWrapper>
    }>
      <BookingSuccessContent />
    </Suspense>
  );
}