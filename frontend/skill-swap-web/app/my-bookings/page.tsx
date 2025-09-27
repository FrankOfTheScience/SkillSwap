"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getCurrentUser } from "../../services/auth";
import { getUserBookings, cancelBooking } from "../../services/booking";
import { Booking, User, BookingStatus } from "../../types";
import BookingCard from "../../components/BookingCard";
import ModalWrapper from "../../components/ModalWrapper";
import DeleteConfirmModal from "../../components/DeleteConfirmModal";

export default function MyBookingsPage() {
  const [user, setUser] = useState<User | null>(null);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [cancelModal, setCancelModal] = useState<number | null>(null);

  const router = useRouter();

  useEffect(() => {
    const currentUser = getCurrentUser();
    if (!currentUser) {
      router.push("/login");
      return;
    }
    setUser(currentUser);
  }, [router]);

  useEffect(() => {
    if (!user) return;

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
  }, [user, page]);

  const handleCancelBooking = async (bookingId: number) => {
    try {
      await cancelBooking(bookingId);
      
      // Update the booking status in the local state
      setBookings(prev => 
        prev.map(booking => 
          booking.id === bookingId 
            ? { ...booking, status: BookingStatus.Cancelled }
            : booking
        )
      );
      
      setCancelModal(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to cancel booking");
    }
  };

  const getBookingStats = () => {
    const stats = {
      total: bookings.length,
      pending: bookings.filter(b => b.status === BookingStatus.Pending).length,
      completed: bookings.filter(b => b.status === BookingStatus.Completed).length,
      cancelled: bookings.filter(b => b.status === BookingStatus.Cancelled).length,
    };
    return stats;
  };

  if (!user) {
    return (
      <ModalWrapper title="Access Denied">
        <div className="text-center py-12">
          <div className="text-6xl mb-4">🔒</div>
          <h2 className="text-xl font-bold text-gray-800 mb-4">Please log in</h2>
          <p className="text-gray-600 mb-6">You need to be logged in to view your bookings.</p>
          <button
            onClick={() => router.push("/login")}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold"
          >
            Go to Login
          </button>
        </div>
      </ModalWrapper>
    );
  }

  const stats = getBookingStats();
  const bookingToCancel = bookings.find(b => b.id === cancelModal);

  return (
    <ModalWrapper title="My Bookings">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex justify-between items-center">
          <h2 className="text-2xl font-bold text-gray-900">
            Welcome, {user.displayName}
          </h2>
          <button
            onClick={() => router.push("/")}
            className="text-blue-600 hover:text-blue-700 font-medium"
          >
            ← Browse Offers
          </button>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-blue-50 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-blue-600">{stats.total}</div>
            <div className="text-sm text-blue-700">Total Bookings</div>
          </div>
          <div className="bg-yellow-50 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-yellow-600">{stats.pending}</div>
            <div className="text-sm text-yellow-700">Pending</div>
          </div>
          <div className="bg-green-50 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-green-600">{stats.completed}</div>
            <div className="text-sm text-green-700">Completed</div>
          </div>
          <div className="bg-gray-50 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-gray-600">{stats.cancelled}</div>
            <div className="text-sm text-gray-700">Cancelled</div>
          </div>
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
        ) : bookings.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-6xl mb-4">📝</div>
            <h3 className="text-xl font-bold text-gray-800 mb-4">No bookings yet</h3>
            <p className="text-gray-600 mb-6">
              You haven&apos;t made any bookings yet. Browse our offers to get started!
            </p>
            <button
              onClick={() => router.push("/")}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold"
            >
              Browse Offers
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {bookings.map(booking => (
              <BookingCard
                key={booking.id}
                booking={booking}
                onCancel={(bookingId) => setCancelModal(bookingId)}
                showActions={true}
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

      {/* Cancel Confirmation Modal */}
      <DeleteConfirmModal
        isOpen={cancelModal !== null}
        onClose={() => setCancelModal(null)}
        onConfirm={() => cancelModal && handleCancelBooking(cancelModal)}
        title="Cancel Booking"
        message={bookingToCancel ? 
          `Are you sure you want to cancel your booking for "${bookingToCancel.offer?.title}"? This action cannot be undone.` :
          "Are you sure you want to cancel this booking?"
        }
      />
    </ModalWrapper>
  );
}