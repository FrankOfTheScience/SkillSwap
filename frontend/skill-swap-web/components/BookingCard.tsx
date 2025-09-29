"use client";
import { useRouter } from "next/navigation";
import { Booking, BookingStatus } from "../types";

interface BookingCardProps {
  booking: Booking;
  onCancel?: (bookingId: number) => void;
  showActions?: boolean;
  onViewOffer?: (offerId: number) => void;
}

export default function BookingCard({ 
  booking, 
  onCancel, 
  showActions = true,
  onViewOffer
}: BookingCardProps) {
  const router = useRouter();
  
  const getStatusColor = (status: BookingStatus) => {
    switch (status) {
      case BookingStatus.Pending:
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      case BookingStatus.Completed:
        return "bg-green-100 text-green-800 border-green-200";
      case BookingStatus.Cancelled:
        return "bg-gray-100 text-gray-800 border-gray-200";
      case BookingStatus.Refunded:
        return "bg-blue-100 text-blue-800 border-blue-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  const getStatusIcon = (status: BookingStatus) => {
    switch (status) {
      case BookingStatus.Pending:
        return "⏳";
      case BookingStatus.Completed:
        return "✅";
      case BookingStatus.Cancelled:
        return "❌";
      case BookingStatus.Refunded:
        return "💰";
      default:
        return "❓";
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const canCancel = booking.status === BookingStatus.Pending;

  return (
    <div className="bg-white rounded-xl shadow-lg border border-gray-100 p-6 hover:shadow-xl transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-bold text-gray-900 mb-1">
            {booking.offer?.title || `Booking #${booking.id}`}
          </h3>
          <p className="text-sm text-gray-600">
            Booking ID: {booking.id}
          </p>
        </div>
        <span className={`px-3 py-1 rounded-full text-sm font-medium border ${getStatusColor(booking.status)}`}>
          {getStatusIcon(booking.status)} {booking.status}
        </span>
      </div>

      {/* Offer Description */}
      {booking.offer && (
        <p className="text-gray-700 text-sm mb-4 line-clamp-2">
          {booking.offer.description}
        </p>
      )}

      {/* Amount and Date Info */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div>
          <p className="text-xs text-gray-500 mb-1">Total Amount</p>
          <p className="text-lg font-bold text-emerald-600">€{booking.amount.toFixed(2)}</p>
          {booking.commissionAmount > 0 && (
            <p className="text-xs text-gray-500">
              (includes €{booking.commissionAmount.toFixed(2)} platform fee)
            </p>
          )}
        </div>
        <div>
          <p className="text-xs text-gray-500 mb-1">Created</p>
          <p className="text-sm font-medium text-gray-900">
            {formatDate(booking.createdAt)}
          </p>
          {booking.completedAt && (
            <>
              <p className="text-xs text-gray-500 mt-2 mb-1">Completed</p>
              <p className="text-sm font-medium text-gray-900">
                {formatDate(booking.completedAt)}
              </p>
            </>
          )}
        </div>
      </div>

      {/* Payment Info */}
      {(booking.stripeCheckoutSessionId || booking.stripePaymentIntentId) && (
        <div className="bg-gray-50 rounded-lg p-3 mb-4">
          <p className="text-xs text-gray-500 mb-2">Payment Information</p>
          {booking.stripeCheckoutSessionId && (
            <p className="text-xs text-gray-700 mb-1">
              <span className="font-medium">Session:</span> {booking.stripeCheckoutSessionId}
            </p>
          )}
          {booking.stripePaymentIntentId && (
            <p className="text-xs text-gray-700">
              <span className="font-medium">Payment:</span> {booking.stripePaymentIntentId}
            </p>
          )}
        </div>
      )}

      {/* Actions */}
      {showActions && (
        <div className="flex gap-3">
          {booking.offer && (
            <button 
              onClick={() => onViewOffer ? onViewOffer(booking.offer!.id) : router.push(`/offers/${booking.offer!.id}/view`)}
              className="flex-1 bg-blue-100 hover:bg-blue-200 text-blue-800 py-2 px-4 rounded-lg font-medium text-sm transition-colors"
            >
              📄 View Offer
            </button>
          )}
          
          {canCancel && onCancel && (
            <button
              onClick={() => onCancel(booking.id)}
              className="bg-red-100 hover:bg-red-200 text-red-800 py-2 px-4 rounded-lg font-medium text-sm transition-colors"
            >
              ❌ Cancel
            </button>
          )}
          
          {booking.status === BookingStatus.Completed && (
            <button className="bg-green-100 hover:bg-green-200 text-green-800 py-2 px-4 rounded-lg font-medium text-sm transition-colors">
              📞 Contact Provider
            </button>
          )}
        </div>
      )}
    </div>
  );
}