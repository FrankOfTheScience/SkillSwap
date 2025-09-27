"use client";
import { useEffect, useState, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getCurrentUser, logout } from "../services/auth";
import OfferList from "../components/OfferList";
import LoginModal from "../components/LoginModal";
import RegisterModal from "../components/RegisterModal";
import CreateOfferModal from "../components/CreateOfferModal";
import { User } from "../types";

function HomePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Modal states
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showCreateOfferModal, setShowCreateOfferModal] = useState(false);

  useEffect(() => {
    const currentUser = getCurrentUser();
    setUser(currentUser);
    setLoading(false);

    // Check URL parameters to show appropriate modal
    const modalParam = searchParams.get('showModal');
    if (modalParam === 'login') {
      setShowLoginModal(true);
    } else if (modalParam === 'register') {
      setShowRegisterModal(true);
    } else if (modalParam === 'createOffer') {
      setShowCreateOfferModal(true);
    }
  }, [searchParams]);

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
          <div className="flex justify-between items-center">
            <div className="flex items-center space-x-4">
              <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-indigo-600 rounded-full flex items-center justify-center">
                <span className="text-white font-bold text-lg">
                  {user.displayName.charAt(0).toUpperCase()}
                </span>
              </div>
              <div>
                <h2 className="text-xl font-semibold text-gray-800">
                  Welcome back, <span className="text-indigo-600">{user.displayName}</span>!
                </h2>
                <div className="flex items-center gap-3 text-gray-600">
                  <span>{user.email}</span>
                  <span>•</span>
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800">
                    {user.role}
                  </span>
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => router.push('/my-bookings')}
                className="bg-gradient-to-r from-purple-500 to-indigo-600 hover:from-purple-600 hover:to-indigo-700 text-white px-6 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md"
              >
                📋 My Bookings
              </button>
              <button 
                onClick={handleLogout}
                className="bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 text-white px-6 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md"
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
        <OfferList user={user} />
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
