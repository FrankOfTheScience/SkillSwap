"use client";
import { useEffect, useState, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getCurrentUser, logout } from "../services/auth";
import OfferList from "../components/OfferList";
import LoginModal from "../components/LoginModal";
import RegisterModal from "../components/RegisterModal";
import CreateOfferModal from "../components/CreateOfferModal";
import MyBookingsModal from "../components/MyBookingsModal";
import ViewOfferModal from "../components/ViewOfferModal";
import EditOfferModal from "../components/EditOfferModal";
import BookingSuccessModal from "../components/BookingSuccessModal";
import BookingConfirmModal from "../components/BookingConfirmModal";
import { User, Offer } from "../types";

function HomePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Modal states
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showCreateOfferModal, setShowCreateOfferModal] = useState(false);
  const [showMyBookingsModal, setShowMyBookingsModal] = useState(false);
  const [showViewOfferModal, setShowViewOfferModal] = useState(false);
  const [showEditOfferModal, setShowEditOfferModal] = useState(false);
  const [showBookingSuccessModal, setShowBookingSuccessModal] = useState(false);
  const [showBookingConfirmModal, setShowBookingConfirmModal] = useState(false);
  const [selectedOfferId, setSelectedOfferId] = useState<number | null>(null);
  const [cameFromMyBookings, setCameFromMyBookings] = useState(false);
  const [selectedOffer, setSelectedOffer] = useState<Offer | null>(null);

  useEffect(() => {
    const currentUser = getCurrentUser();
    setUser(currentUser);
    setLoading(false);

    // Check URL parameters to show appropriate modal
    const modalParam = searchParams.get('showModal');
    const sessionId = searchParams.get('session_id');
    
    if (modalParam === 'login') {
      setShowLoginModal(true);
    } else if (modalParam === 'register') {
      setShowRegisterModal(true);
    } else if (modalParam === 'createOffer') {
      setShowCreateOfferModal(true);
    } else if (modalParam === 'bookingSuccess' || sessionId) {
      // Show booking success modal if coming from Stripe
      setShowBookingSuccessModal(true);
      // Clean up URL
      router.replace('/', { scroll: false });
    }
  }, [searchParams, router]);

  const handleLogout = () => {
    logout();
    setUser(null);
  };

  const handleModalClose = () => {
    setShowLoginModal(false);
    setShowRegisterModal(false);
    setShowCreateOfferModal(false);
    // Clean the URL without page reload
    router.replace("/", { scroll: false });
  };

  const handleAuthSuccess = () => {
    // Refresh user state and close modal
    const currentUser = getCurrentUser();
    setUser(currentUser);
    handleModalClose();
  };

  const handleCreateOfferSuccess = () => {
    // Close modal and refresh the page to show new offer
    handleModalClose();
    window.location.reload();
  };

  const handleSwitchToRegister = () => {
    setShowLoginModal(false);
    setShowRegisterModal(true);
    router.replace("/?showModal=register", { scroll: false });
  };

  const handleSwitchToLogin = () => {
    setShowRegisterModal(false);
    setShowLoginModal(true);
    router.replace("/?showModal=login", { scroll: false });
  };

  const handleViewOffer = (offerId: number) => {
    setSelectedOfferId(offerId);
    setShowViewOfferModal(true);
    // Check if MyBookings modal is open
    if (showMyBookingsModal) {
      setCameFromMyBookings(true);
      setShowMyBookingsModal(false);
    }
  };

  const handleEditOffer = (offerId: number) => {
    setSelectedOfferId(offerId);
    setShowEditOfferModal(true);
    setShowViewOfferModal(false);
  };

  const handleCloseViewOffer = () => {
    setShowViewOfferModal(false);
    setSelectedOfferId(null);
    setCameFromMyBookings(false);
  };

  const handleCloseEditOffer = () => {
    setShowEditOfferModal(false);
    setSelectedOfferId(null);
  };

  const handleEditOfferSuccess = () => {
    // Refresh the offer list or update the offer
    setShowEditOfferModal(false);
    setSelectedOfferId(null);
  };

  const handleBookingSuccessViewBookings = () => {
    setShowBookingSuccessModal(false);
    setShowMyBookingsModal(true);
  };

  const handleBookOffer = (offer: Offer) => {
    setSelectedOffer(offer);
    setShowBookingConfirmModal(true);
  };

  const handleBackToMyBookings = () => {
    setShowViewOfferModal(false);
    setShowMyBookingsModal(true);
    setCameFromMyBookings(false);
  };

  if (loading) {
    return (
      <main className="p-8">
        <div className="text-center">Loading...</div>
      </main>
    );
  }

  return (
    <main className="p-8">
      {/* Authentication Section */}
      <div className="mb-8 p-6 bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 rounded-xl border border-blue-100 shadow-lg">
        {user ? (
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="flex items-center space-x-3 sm:space-x-4 min-w-0 flex-1">
              <div className="w-10 h-10 sm:w-12 sm:h-12 bg-gradient-to-r from-blue-500 to-indigo-600 rounded-full flex items-center justify-center flex-shrink-0">
                <span className="text-white font-bold text-base sm:text-lg">
                  {user.displayName.charAt(0).toUpperCase()}
                </span>
              </div>
              <div className="min-w-0 flex-1">
                <h2 className="text-lg sm:text-xl font-semibold text-gray-800 truncate">
                  Welcome back, <span className="text-indigo-600">{user.displayName}</span>!
                </h2>
                <div className="flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-3 text-gray-600 text-xs sm:text-sm">
                  <span className="truncate">{user.email}</span>
                  <span className="hidden sm:inline">•</span>
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800 flex-shrink-0 w-fit">
                    {user.role}
                  </span>
                </div>
              </div>
            </div>
            <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
              <button
                onClick={() => setShowMyBookingsModal(true)}
                className="bg-gradient-to-r from-purple-500 to-indigo-600 hover:from-purple-600 hover:to-indigo-700 text-white px-6 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md text-center whitespace-nowrap"
              >
                📋 My Bookings
              </button>
              <button 
                onClick={handleLogout}
                className="bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 text-white px-6 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md text-center whitespace-nowrap"
              >
                👋 Logout
              </button>
            </div>
          </div>
        ) : (
          <div className="text-center">
            <div className="mb-4">
              <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-600 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-white text-2xl">🤝</span>
              </div>
              <h2 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-2">
                Welcome to SkillSwap!
              </h2>
              <p className="text-gray-600 text-lg max-w-md mx-auto">
                Join our vibrant community to share your expertise and discover new skills from talented individuals around the world.
              </p>
            </div>
            <div className="flex justify-center gap-4 mt-6">
              <button
                onClick={() => setShowLoginModal(true)}
                className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
              >
                🔐 Login
              </button>
              <button
                onClick={() => setShowRegisterModal(true)}
                className="bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
              >
                ✨ Get Started
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Offers Section */}
      <div>
        {user && (
          <div className="mb-8 p-6 bg-gradient-to-r from-emerald-50 to-teal-50 rounded-xl border border-emerald-100 shadow-lg">
            <div className="flex justify-between items-center">
              <div>
                <h2 className="text-2xl font-bold text-emerald-800 mb-2">
                  🚀 Ready to Share Your Skills?
                </h2>
                <p className="text-emerald-600">
                  Create a new offer and connect with people who need your expertise!
                </p>
              </div>
              <button
                onClick={() => setShowCreateOfferModal(true)}
                className="bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
              >
                ✨ Create New Offer
              </button>
            </div>
          </div>
        )}
        
        <h1 className="text-3xl font-bold mb-6">Available Offers</h1>
        <OfferList user={user} onViewOffer={handleViewOffer} />
      </div>

      {/* Modals */}
      <LoginModal
        isOpen={showLoginModal}
        onClose={handleModalClose}
        onSuccess={handleAuthSuccess}
        onSwitchToRegister={handleSwitchToRegister}
      />

      <RegisterModal
        isOpen={showRegisterModal}
        onClose={handleModalClose}
        onSuccess={handleAuthSuccess}
        onSwitchToLogin={handleSwitchToLogin}
      />

      {user && (
        <CreateOfferModal
          isOpen={showCreateOfferModal}
          onClose={handleModalClose}
          onSuccess={handleCreateOfferSuccess}
        />
      )}

      <MyBookingsModal
        isOpen={showMyBookingsModal}
        onClose={() => setShowMyBookingsModal(false)}
        onViewOffer={handleViewOffer}
      />

      <ViewOfferModal
        isOpen={showViewOfferModal}
        onClose={handleCloseViewOffer}
        offerId={selectedOfferId}
        onEdit={handleEditOffer}
        onBack={cameFromMyBookings ? handleBackToMyBookings : undefined}
        onBookOffer={handleBookOffer}
      />

      <EditOfferModal
        isOpen={showEditOfferModal}
        onClose={handleCloseEditOffer}
        offerId={selectedOfferId}
        onSuccess={handleEditOfferSuccess}
      />

      <BookingSuccessModal
        isOpen={showBookingSuccessModal}
        onClose={() => setShowBookingSuccessModal(false)}
        onViewMyBookings={handleBookingSuccessViewBookings}
      />

      {selectedOffer && user && (
        <BookingConfirmModal
          isOpen={showBookingConfirmModal}
          onClose={() => setShowBookingConfirmModal(false)}
          offer={selectedOffer}
          user={user}
        />
      )}
    </main>
  );
}

export default function Home() {
  return (
    <Suspense fallback={<div className="p-8 text-center">Loading...</div>}>
      <HomePage />
    </Suspense>
  );
}
