"use client";
import { useEffect, useState } from "react";
import api from "../services/api";
import { Offer, User } from "../types";
import { getCurrentUser } from "../services/auth";
import DeleteConfirmModal from "./DeleteConfirmModal";
import ModalWrapper from "./ModalWrapper";
import BookingConfirmModal from "./BookingConfirmModal";

interface ViewOfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  offerId: number | null;
  onEdit?: (offerId: number) => void;
  onBack?: () => void;
  onBookOffer?: (offer: any) => void;
}

export default function ViewOfferModal({ isOpen, onClose, offerId, onEdit, onBack, onBookOffer }: ViewOfferModalProps) {
  const [offer, setOffer] = useState<Offer | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleteModal, setDeleteModal] = useState(false);
  const [bookingModal, setBookingModal] = useState(false);

  useEffect(() => {
    if (!isOpen || !offerId) {
      setOffer(null);
      setLoading(true);
      return;
    }

    const fetchData = async () => {
      try {
        setUser(getCurrentUser());
        const response = await api.get(`/api/offers/${offerId}`);
        setOffer(response.data);
      } catch (error) {
        console.error("Failed to fetch offer:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [isOpen, offerId]);

  const handleDeleteClick = () => {
    setDeleteModal(true);
  };

  const handleBookingClick = () => {
    if (onBookOffer && offer) {
      onBookOffer(offer);
      onClose();
    } else {
      setBookingModal(true);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!offer) return;
    
    try {
      await api.delete(`/api/offers/${offer.id}`);
      setDeleteModal(false);
      onClose();
    } catch (error) {
      console.error("Delete failed:", error);
    }
  };

  const handleEditClick = () => {
    if (offer && onEdit) {
      onEdit(offer.id);
      onClose();
    }
  };

  const isOwner = offer && user?.id === offer.createdBy;
  const isAdmin = user?.role === "Admin";

  if (!isOpen) return null;

  if (loading) {
    return (
      <ModalWrapper title="Offer Details" isOpen={isOpen} onClose={onClose}>
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading offer details...</p>
        </div>
      </ModalWrapper>
    );
  }

  if (!offer) {
    return (
      <ModalWrapper title="Offer Not Found" isOpen={isOpen} onClose={onClose}>
        <div className="text-center py-8">
          <div className="text-6xl mb-4">❌</div>
          <h2 className="text-xl font-bold text-gray-800 mb-4">Offer not found</h2>
          <p className="text-gray-600 mb-6">The offer you&apos;re looking for doesn&apos;t exist or has been removed.</p>
          <button
            onClick={onClose}
            className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105"
          >
            Close
          </button>
        </div>
      </ModalWrapper>
    );
  }

  return (
    <ModalWrapper title="Offer Details" isOpen={isOpen} onClose={onClose}>
      <div className="bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 rounded-xl border border-blue-100 shadow-lg p-6">
        <div className="mb-6">
          <h2 className="text-3xl font-bold text-gray-900 mb-4">{offer.title}</h2>
          <p className="text-gray-700 text-lg leading-relaxed mb-6">{offer.description}</p>
          <div className="flex items-center justify-between">
            <p className="text-4xl font-bold text-emerald-600">
              €{offer.price}
            </p>
            
            {user && isOwner && (
              <span className="inline-flex items-center px-4 py-2 rounded-full text-sm font-medium bg-emerald-100 text-emerald-800">
                👤 Your Offer
              </span>
            )}
          </div>
        </div>

        <div className="flex gap-4 justify-center pt-4">
          {user && (isOwner || isAdmin) ? (
            <>
              <button
                onClick={handleEditClick}
                className="bg-gradient-to-r from-yellow-400 to-orange-500 hover:from-yellow-500 hover:to-orange-600 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center gap-2"
              >
                ✏️ Update Offer
              </button>
              <button
                onClick={handleDeleteClick}
                className="bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 text-white px-6 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center gap-2"
              >
                🗑️ Delete Offer
              </button>
            </>
          ) : user ? (
            <button 
              onClick={handleBookingClick}
              className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center gap-2"
            >
              📅 Book This Offer
            </button>
          ) : (
            <button
              onClick={onClose}
              className="bg-gradient-to-r from-gray-400 to-gray-500 hover:from-gray-500 hover:to-gray-600 text-white px-8 py-3 rounded-lg font-semibold transition-all duration-200 transform hover:scale-105 shadow-lg flex items-center gap-2"
            >
              🔐 Login to Book
            </button>
          )}
        </div>

        {/* Back button if coming from My Bookings */}
        {onBack && (
          <div className="flex justify-center pt-4 border-t border-gray-200 mt-6">
            <button
              onClick={onBack}
              className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-6 py-2 rounded-lg font-medium transition-colors flex items-center gap-2"
            >
              ← Back to My Bookings
            </button>
          </div>
        )}
      </div>

      <DeleteConfirmModal
        isOpen={deleteModal}
        onClose={() => setDeleteModal(false)}
        onConfirm={handleDeleteConfirm}
        title="Delete Offer"
        message={`Are you sure you want to delete "${offer.title}"? This action cannot be undone.`}
      />
    </ModalWrapper>
  );
}