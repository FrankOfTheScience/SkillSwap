"use client";
import { useEffect, useState } from "react";
import api from "../services/api";
import { Offer, User } from "../types";
import Link from "next/link";
import DeleteConfirmModal from "./DeleteConfirmModal";
import ErrorDisplay from "./ErrorDisplay";
import { getErrorMessage } from "../utils/errorHandler";
import { createCheckoutSession, redirectToCheckout } from "../services/booking";
import Toast, { useToast } from "./Toast";

interface OfferListProps {
  user: User | null;
}

interface PagedOffersResult {
  offers: Offer[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export default function OfferList({ user }: OfferListProps) {
  const [offersResult, setOffersResult] = useState<PagedOffersResult>({
    offers: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    totalPages: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Filter states
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [maxBudget, setMaxBudget] = useState<number | undefined>();
  const [showOnlyMyOffers, setShowOnlyMyOffers] = useState<boolean | undefined>();
  const [sortBy, setSortBy] = useState("id");
  const [sortDescending, setSortDescending] = useState(false);
  
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [bookingLoading, setBookingLoading] = useState<Record<number, boolean>>({});
  const [bookingError, setBookingError] = useState<Record<number, string>>({});
  const { toast, showToast, hideToast } = useToast();
  
  const [deleteModal, setDeleteModal] = useState<{isOpen: boolean, offer: Offer | null}>({
    isOpen: false,
    offer: null
  });

  const loadOffers = async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());
      if (search) params.append('search', search);
      if (maxBudget !== undefined) params.append('maxBudget', maxBudget.toString());
      if (showOnlyMyOffers !== undefined) params.append('showOnlyMyOffers', showOnlyMyOffers.toString());
      params.append('sortBy', sortBy);
      params.append('sortDescending', sortDescending.toString());

      const response = await api.get(`/offers?${params.toString()}`);
      setOffersResult(response.data);
    } catch (error) {
      const errorMessage = getErrorMessage(error);
      setError(errorMessage);
      console.error("Failed to load offers:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadOffers();
  }, [page, pageSize, search, maxBudget, showOnlyMyOffers, sortBy, sortDescending]); // loadOffers is recreated on every render, so we omit it to avoid infinite loops

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1); // Reset to first page when searching
    loadOffers();
  };

  const handleBudgetChange = (value: string) => {
    const budget = value === "" ? undefined : parseFloat(value);
    setMaxBudget(budget);
    setPage(1);
  };

  const handleDeleteClick = (offer: Offer) => {
    setDeleteModal({ isOpen: true, offer });
  };

  const handleDeleteConfirm = async () => {
    if (!deleteModal.offer) return;
    
    setDeleteError(null);
    try {
      await api.delete(`/offers/${deleteModal.offer.id}`);
      setOffersResult(prev => ({
        ...prev,
        offers: prev.offers.filter(o => o.id !== deleteModal.offer!.id),
        totalCount: prev.totalCount - 1
      }));
      setDeleteModal({ isOpen: false, offer: null });
    } catch (error) {
      const errorMessage = getErrorMessage(error);
      setDeleteError(errorMessage);
      console.error("Delete failed:", error);
      // Keep modal open to show error
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModal({ isOpen: false, offer: null });
    setDeleteError(null);
  };

  const isOwner = (offer: Offer) => {
    console.log('Checking ownership:', { userId: user?.id, offerCreatedBy: offer.createdBy, userObj: user });
    return user?.id === offer.createdBy;
  };
  
  const isAdmin = () => {
    console.log('Checking admin status:', { userRole: user?.role, user });
    return user?.role === "Admin" || (user as { roles?: string[] })?.roles?.includes("Admin");
  };

  const handleBookOffer = async (offer: Offer) => {
    if (!user) {
      // This shouldn't happen as the button is only shown when user is logged in
      return;
    }

    // Clear any previous error for this offer
    setBookingError(prev => ({ ...prev, [offer.id]: "" }));
    setBookingLoading(prev => ({ ...prev, [offer.id]: true }));

    try {
      const checkoutSession = await createCheckoutSession({
        offerId: offer.id,
        userId: user.id
      });

      showToast(`Redirecting to payment for "${offer.title}"...`, 'info', 2000);
      
      // Small delay to show the toast before redirect
      setTimeout(() => {
        redirectToCheckout(checkoutSession.checkoutUrl);
      }, 500);
    } catch (error) {
      const errorMessage = getErrorMessage(error);
      setBookingError(prev => ({ ...prev, [offer.id]: errorMessage }));
      showToast(`Booking failed: ${errorMessage}`, 'error');
      console.error("Booking failed:", error);
    } finally {
      setBookingLoading(prev => ({ ...prev, [offer.id]: false }));
    }
  };

  return (
    <div className="space-y-6">
      {/* Filters Section */}
      <div className="bg-white p-6 rounded-xl border-2 border-gray-100 shadow-lg">
        <h3 className="text-lg font-bold text-gray-800 mb-4">🔍 Search & Filter Offers</h3>
        
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Search */}
          <form onSubmit={handleSearchSubmit} className="lg:col-span-3">
            <div className="flex gap-2">
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search in titles and descriptions..."
                className="flex-1 px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900 placeholder-gray-500"
              />
              <button
                type="submit"
                className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
              >
                Search
              </button>
            </div>
          </form>

          {/* Budget Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Max Budget (€)</label>
            <input
              type="number"
              min="0"
              step="0.01"
              value={maxBudget || ""}
              onChange={(e) => handleBudgetChange(e.target.value)}
              placeholder="Any budget"
              className="w-full px-3 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900 placeholder-gray-500"
            />
          </div>

          {/* Ownership Filter */}
          {user && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Show Offers</label>
              <select
                value={showOnlyMyOffers === undefined ? "all" : showOnlyMyOffers ? "mine" : "others"}
                onChange={(e) => {
                  const value = e.target.value;
                  setShowOnlyMyOffers(value === "all" ? undefined : value === "mine");
                  setPage(1);
                }}
                className="w-full px-3 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900"
              >
                <option value="all">All Offers</option>
                <option value="mine">My Offers</option>
                <option value="others">Others&apos; Offers</option>
              </select>
            </div>
          )}

          {/* Sort Options */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Sort By</label>
            <div className="flex gap-2">
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="flex-1 px-3 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900"
              >
                <option value="id">Date Created</option>
                <option value="title">Title</option>
                <option value="price">Price</option>
              </select>
              <button
                type="button"
                onClick={() => setSortDescending(!sortDescending)}
                className={`px-3 py-2 rounded-lg border-2 transition-colors ${
                  sortDescending 
                    ? 'bg-blue-500 text-white border-blue-500' 
                    : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                }`}
              >
                {sortDescending ? "↓" : "↑"}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Error Display */}
      {error && (
        <ErrorDisplay
          error={error}
          title="Unable to load offers"
          onRetry={() => {
            setError(null);
            loadOffers();
          }}
        />
      )}

      {/* Results Info */}
      <div className="flex justify-between items-center">
        <p className="text-gray-600">
          {loading ? "Loading..." : `Showing ${offersResult.offers.length} of ${offersResult.totalCount} offers`}
        </p>
        <div className="flex items-center gap-2">
          <label className="text-sm text-gray-600">Per page:</label>
          <select
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPage(1);
            }}
            className="px-2 py-1 border border-gray-300 rounded text-sm"
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
        </div>
      </div>

      {/* Offers Grid */}
      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading offers...</p>
        </div>
      ) : !error ? (
        <div className="grid gap-6">
          {offersResult.offers.map(offer => (
            <div key={offer.id} className="p-6 bg-white border-2 border-gray-100 rounded-xl shadow-lg hover:shadow-xl hover:border-gray-200 transition-all duration-300">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <Link href={`/offers/${offer.id}/view`}>
                    <h3 className="text-2xl font-bold text-gray-900 mb-3 hover:text-indigo-600 transition-colors cursor-pointer">
                      {offer.title}
                    </h3>
                  </Link>
                  <p className="text-gray-700 mb-4 leading-relaxed">{offer.description}</p>
                  <div className="flex items-center justify-between">
                    <p className="text-3xl font-bold bg-gradient-to-r from-emerald-600 to-teal-600 bg-clip-text text-transparent">
                      {offer.price} €
                    </p>
                    
                    <div className="flex gap-3">
                      {user && (isOwner(offer) || isAdmin()) ? (
                        <>
                          <Link
                            href={`/offers/${offer.id}/edit`}
                            className="bg-gradient-to-r from-yellow-400 to-orange-500 hover:from-yellow-500 hover:to-orange-600 text-white px-4 py-2 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2"
                          >
                            ✏️ Edit
                          </Link>
                          <button
                            onClick={() => handleDeleteClick(offer)}
                            className="bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 text-white px-4 py-2 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2"
                          >
                            🗑️ Delete
                          </button>
                        </>
                      ) : user ? (
                        <div className="flex flex-col gap-2">
                          <button 
                            onClick={() => handleBookOffer(offer)}
                            disabled={bookingLoading[offer.id]}
                            className={`px-6 py-2 rounded-lg font-semibold transition-all duration-200 transform shadow-md flex items-center gap-2 justify-center ${
                              bookingLoading[offer.id]
                                ? "bg-gray-400 cursor-not-allowed"
                                : "bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white hover:scale-105"
                            }`}
                          >
                            {bookingLoading[offer.id] ? (
                              <>
                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                Processing...
                              </>
                            ) : (
                              <>📅 Book for €{offer.price}</>
                            )}
                          </button>
                          {bookingError[offer.id] && (
                            <div className="bg-red-50 border border-red-200 rounded-lg p-2 text-red-700 text-sm">
                              <div className="flex items-center gap-2">
                                <span>❌</span>
                                <span>{bookingError[offer.id]}</span>
                              </div>
                              <button 
                                onClick={() => setBookingError(prev => ({ ...prev, [offer.id]: "" }))}
                                className="text-xs underline mt-1 hover:no-underline"
                              >
                                Dismiss
                              </button>
                            </div>
                          )}
                        </div>
                      ) : (
                        <Link
                          href="/?showModal=login"
                          className="bg-gradient-to-r from-gray-400 to-gray-500 hover:from-gray-500 hover:to-gray-600 text-white px-6 py-2 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2"
                        >
                          🔐 Login to book
                        </Link>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : null}

      {/* Empty State */}
      {!loading && !error && offersResult.offers.length === 0 && (
        <div className="text-center py-12">
          <div className="text-6xl mb-4">📋</div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">No offers found</h3>
          <p className="text-gray-600">
            {search || maxBudget || showOnlyMyOffers !== undefined 
              ? "Try adjusting your search filters to see more results."
              : "No offers available at the moment."}
          </p>
        </div>
      )}

      {/* Pagination */}
      {!loading && !error && offersResult.totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 py-6">
          <button
            onClick={() => setPage(Math.max(1, page - 1))}
            disabled={page === 1}
            className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            ← Previous
          </button>
          
          <div className="flex gap-1">
            {Array.from({ length: Math.min(5, offersResult.totalPages) }, (_, i) => {
              const pageNum = Math.max(1, Math.min(
                offersResult.totalPages - 4,
                Math.max(1, page - 2)
              )) + i;
              
              if (pageNum <= offersResult.totalPages) {
                return (
                  <button
                    key={pageNum}
                    onClick={() => setPage(pageNum)}
                    className={`px-3 py-2 rounded-lg ${
                      page === pageNum
                        ? 'bg-blue-500 text-white'
                        : 'bg-white border border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    {pageNum}
                  </button>
                );
              }
              return null;
            })}
          </div>
          
          <button
            onClick={() => setPage(Math.min(offersResult.totalPages, page + 1))}
            disabled={page === offersResult.totalPages}
            className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Next →
          </button>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      <DeleteConfirmModal
        isOpen={deleteModal.isOpen}
        onClose={handleDeleteCancel}
        onConfirm={handleDeleteConfirm}
        title="Delete Offer"
        message={deleteModal.offer ? `Are you sure you want to delete "${deleteModal.offer.title}"? This action cannot be undone.` : ""}
        error={deleteError}
      />

      {/* Toast Notifications */}
      <Toast
        message={toast.message}
        type={toast.type}
        isVisible={toast.isVisible}
        onClose={hideToast}
      />
    </div>
  );
}