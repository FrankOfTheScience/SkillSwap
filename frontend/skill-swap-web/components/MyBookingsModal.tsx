"use client";
import { useEffect, useState } from "react";
import { getCurrentUser } from "../services/auth";
import { getUserBookings } from "../services/booking";
import { Booking, User, BookingStatus } from "../types";
import BookingCard from "./BookingCard";
import ModalWrapper from "./ModalWrapper";

interface MyBookingsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onViewOffer?: (offerId: string) => void;
  onCancelBooking?: (bookingId: string) => void;
}

export default function MyBookingsModal({ isOpen, onClose, onViewOffer, onCancelBooking }: MyBookingsModalProps) {
  const [user, setUser] = useState<User | null>(null);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [statusFilter, setStatusFilter] = useState<BookingStatus | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    
    const currentUser = getCurrentUser();
    if (!currentUser) {
      onClose();
      return;
    }
    setUser(currentUser);
  }, [isOpen, onClose]);

  useEffect(() => {
    if (!user || !isOpen) return;

    const fetchBookings = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await getUserBookings(page, 10);
        setBookings(response.data);
        setTotalPages(response.totalPages);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load bookings");
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, [user, page, isOpen]);

  const getBookingStats = () => {
    const stats = {
      total: bookings.length,
      pending: bookings.filter(b => b.status === BookingStatus.Pending).length,
      completed: bookings.filter(b => b.status === BookingStatus.Completed).length,
      cancelled: bookings.filter(b => b.status === BookingStatus.Cancelled).length,
    };
    return stats;
  };

  const getFilteredBookings = () => {
    if (statusFilter === null) {
      return bookings;
    }
    return bookings.filter(b => b.status === statusFilter);
  };

  const handleStatusFilter = (status: BookingStatus | null) => {
    setStatusFilter(status);
  };

  if (!user) {
    return (
      <ModalWrapper title="Access Denied" isOpen={isOpen} onClose={onClose}>
        <div className="text-center py-12">
          <div className="text-6xl mb-4">🔒</div>
          <h2 className="text-xl font-bold text-gray-800 mb-4">Please log in</h2>
          <p className="text-gray-600 mb-6">You need to be logged in to view your bookings.</p>
          <button
            onClick={onClose}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold"
          >
            Close
          </button>
        </div>
      </ModalWrapper>
    );
  }

  const stats = getBookingStats();
  const filteredBookings = getFilteredBookings();

  return (
    <ModalWrapper title="My Bookings" isOpen={isOpen} onClose={onClose}>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex justify-between items-center">
          <h2 className="text-2xl font-bold text-gray-900">
            Welcome, {user.displayName}
          </h2>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button
            onClick={() => handleStatusFilter(null)}
            className={`bg-blue-50 rounded-lg p-4 text-center hover:bg-blue-100 transition-colors ${
              statusFilter === null ? 'ring-2 ring-blue-500' : ''
            }`}
          >
            <div className="text-2xl font-bold text-blue-600">{stats.total}</div>
            <div className="text-sm text-blue-700">Total Bookings</div>
          </button>
          <button
            onClick={() => handleStatusFilter(BookingStatus.Pending)}
            className={`bg-yellow-50 rounded-lg p-4 text-center hover:bg-yellow-100 transition-colors ${
              statusFilter === BookingStatus.Pending ? 'ring-2 ring-yellow-500' : ''
            }`}
          >
            <div className="text-2xl font-bold text-yellow-600">{stats.pending}</div>
            <div className="text-sm text-yellow-700">Pending</div>
          </button>
          <button
            onClick={() => handleStatusFilter(BookingStatus.Completed)}
            className={`bg-green-50 rounded-lg p-4 text-center hover:bg-green-100 transition-colors ${
              statusFilter === BookingStatus.Completed ? 'ring-2 ring-green-500' : ''
            }`}
          >
            <div className="text-2xl font-bold text-green-600">{stats.completed}</div>
            <div className="text-sm text-green-700">Completed</div>
          </button>
          <button
            onClick={() => handleStatusFilter(BookingStatus.Cancelled)}
            className={`bg-gray-50 rounded-lg p-4 text-center hover:bg-gray-100 transition-colors ${
              statusFilter === BookingStatus.Cancelled ? 'ring-2 ring-gray-500' : ''
            }`}
          >
            <div className="text-2xl font-bold text-gray-600">{stats.cancelled}</div>
            <div className="text-sm text-gray-700">Cancelled</div>
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <div className="flex items-center">
              <span className="text-red-600 text-xl mr-2">⚠️</span>
              <p className="text-red-700">{error}</p>
            </div>
          </div>
        )}

        {/* Bookings List */}
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading your bookings...</p>
          </div>
        ) : filteredBookings.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-6xl mb-4">📝</div>
            <h3 className="text-xl font-bold text-gray-800 mb-4">
              {statusFilter ? `No ${statusFilter.toLowerCase()} bookings` : 'No bookings yet'}
            </h3>
            <p className="text-gray-600 mb-6">
              {statusFilter 
                ? `You don't have any ${statusFilter.toLowerCase()} bookings.`
                : "You haven't made any bookings yet. Browse our offers to get started!"
              }
            </p>
            {!statusFilter && (
              <button
                onClick={onClose}
                className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold"
              >
                Browse Offers
              </button>
            )}
          </div>
        ) : (
          <div className="space-y-4">
            {filteredBookings.map(booking => (
              <BookingCard
                key={booking.id}
                booking={booking}
                onCancel={onCancelBooking}
                showActions={true}
                onViewOffer={onViewOffer}
              />
            ))}
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex justify-center gap-2">
            <button
              onClick={() => setPage(prev => Math.max(1, prev - 1))}
              disabled={page === 1 || loading}
              className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <span className="px-4 py-2 text-gray-700">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage(prev => Math.min(totalPages, prev + 1))}
              disabled={page === totalPages || loading}
              className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
        )}
      </div>
    </ModalWrapper>
  );
}