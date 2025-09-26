"use client";
import { useEffect, useState } from "react";
import api, { getOffers } from "../services/api";
import { Offer, User } from "../types";
import Link from "next/link";
import DeleteConfirmModal from "./DeleteConfirmModal";

interface OfferListProps {
  user: User | null;
}

export default function OfferList({ user }: OfferListProps) {
  const [offers, setOffers] = useState<Offer[]>([]);
  const [deleteModal, setDeleteModal] = useState<{isOpen: boolean, offer: Offer | null}>({
    isOpen: false,
    offer: null
  });

  useEffect(() => {
    getOffers(1, 10).then(data => setOffers(data));
  }, []);

  const handleDeleteClick = (offer: Offer) => {
    setDeleteModal({ isOpen: true, offer });
  };

  const handleDeleteConfirm = async () => {
    if (!deleteModal.offer) return;
    
    try {
      await api.delete(`/offers/${deleteModal.offer.id}`);
      setOffers(offers.filter(o => o.id !== deleteModal.offer!.id));
      setDeleteModal({ isOpen: false, offer: null });
    } catch (error: unknown) {
      const err = error as { response?: { data?: unknown; status?: number }; message?: string };
      console.error("Delete failed:", {
        message: err.message,
        response: err.response,
        status: err.response?.status,
        data: err.response?.data
      });
      // You could show an error message to the user here
      setDeleteModal({ isOpen: false, offer: null });
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModal({ isOpen: false, offer: null });
  };

  const isOwner = (offer: Offer) => {
    console.log('Checking ownership:', { userId: user?.id, offerCreatedBy: offer.createdBy, userObj: user });
    return user?.id === offer.createdBy;
  };
  
  const isAdmin = () => {
    console.log('Checking admin status:', { userRole: user?.role, user });
    return user?.role === "Admin" || (user as { roles?: string[] })?.roles?.includes("Admin");
  };

  return (
    <>
      <div className="grid gap-6">
        {offers.map(o => (
          <div key={o.id} className="p-6 bg-white border-2 border-gray-100 rounded-xl shadow-lg hover:shadow-xl hover:border-gray-200 transition-all duration-300">
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <Link href={`/offers/${o.id}/view`}>
                  <h3 className="text-2xl font-bold text-gray-900 mb-3 hover:text-indigo-600 transition-colors cursor-pointer">
                    {o.title}
                  </h3>
                </Link>
                <p className="text-gray-700 mb-4 leading-relaxed">{o.description}</p>
                <div className="flex items-center justify-between">
                  <p className="text-3xl font-bold bg-gradient-to-r from-emerald-600 to-teal-600 bg-clip-text text-transparent">
                    {o.price} €
                  </p>
                  
                  {/* Show ownership indicator */}
                  {user && isOwner(o) && (
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-emerald-100 text-emerald-800">
                      👤 Your Offer
                    </span>
                  )}
                </div>
              </div>
              
              <div className="flex flex-col gap-3 ml-6">
                {/* Show Update/Delete buttons if user owns the offer OR is Admin */}
                {user && (isOwner(o) || isAdmin()) ? (
                  <div className="flex gap-3">
                    <Link 
                      href={`/offers/${o.id}/edit`} 
                      className="bg-gradient-to-r from-yellow-400 to-orange-500 hover:from-yellow-500 hover:to-orange-600 text-white px-5 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2"
                    >
                      ✏️ Update
                    </Link>
                    <button 
                      onClick={() => handleDeleteClick(o)} 
                      className="bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 text-white px-5 py-2 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2"
                    >
                      🗑️ Delete
                    </button>
                  </div>
                ) : user ? (
                  /* Show Book button if user is logged in but doesn't own the offer */
                  <button className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2">
                    📅 Book Now
                  </button>
                ) : (
                  /* Show Book button even if user is not logged in */
                  <button className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 transform hover:scale-105 shadow-md flex items-center gap-2">
                    � Book Now
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
        
        {offers.length === 0 && (
          <div className="text-center py-16 bg-gradient-to-br from-gray-50 to-blue-50 rounded-xl border-2 border-dashed border-gray-300">
            <div className="text-8xl mb-6">📋</div>
            <h3 className="text-2xl font-bold text-gray-700 mb-3">No offers available yet</h3>
            <p className="text-gray-500 text-lg">Be the first to create an offer and share your skills!</p>
            {user && (
              <Link 
                href="/offers/create"
                className="inline-block mt-6 bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg"
              >
                ✨ Create Your First Offer
              </Link>
            )}
          </div>
        )}
      </div>

      <DeleteConfirmModal
        isOpen={deleteModal.isOpen}
        onClose={handleDeleteCancel}
        onConfirm={handleDeleteConfirm}
        title="Delete Offer"
        message={`Are you sure you want to delete "${deleteModal.offer?.title}"? This action cannot be undone.`}
      />
    </>
  );
}
