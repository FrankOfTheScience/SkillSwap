"use client";
import { useState } from "react";
import api from "../services/api";
import ModalWrapper from "./ModalWrapper";

interface CreateOfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export default function CreateOfferModal({ isOpen, onClose, onSuccess }: CreateOfferModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handlePriceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    let value = e.target.value;
    // Replace comma with dot for consistent decimal handling
    value = value.replace(',', '.');
    setPrice(value);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || !description || !price) {
      setError("Please fill in all fields");
      return;
    }

    const numericPrice = parseFloat(price);
    if (isNaN(numericPrice) || numericPrice < 0) {
      setError("Please enter a valid price");
      return;
    }

    setIsSubmitting(true);
    setError("");

    try {
      await api.post("/offers", {
        title,
        description,
        price: numericPrice
      });
      onSuccess();
      // Reset form
      resetForm();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string; error?: string }; status?: number }; message?: string; config?: unknown };
      console.error("Create offer error:", {
        message: error.message,
        response: error.response,
        status: error.response?.status,
        data: error.response?.data,
        config: error.config
      });
      setError(error.response?.data?.message || error.response?.data?.error || error.message || "Failed to create offer");
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetForm = () => {
    setTitle("");
    setDescription("");
    setPrice("");
    setError("");
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  return (
    <ModalWrapper title="Create New Offer" isOpen={isOpen} onClose={handleClose}>
      <div className="bg-gradient-to-br from-emerald-50 via-teal-50 to-blue-50 rounded-xl border border-emerald-100 shadow-lg p-6">
        <div className="text-center mb-6">
          <div className="w-16 h-16 bg-gradient-to-r from-emerald-500 to-teal-600 rounded-full flex items-center justify-center mx-auto mb-4">
            <span className="text-white text-2xl">✨</span>
          </div>
          <p className="text-gray-600">
            Share your skills with the SkillSwap community
          </p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label htmlFor="title" className="block text-sm font-bold text-gray-800 mb-2">
              Offer Title *
            </label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g., Web Development Tutoring"
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
          </div>

          <div>
            <label htmlFor="description" className="block text-sm font-bold text-gray-800 mb-2">
              Description *
            </label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe what you're offering, your experience, and what students can expect to learn..."
              rows={5}
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors resize-vertical bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
          </div>

          <div>
            <label htmlFor="price" className="block text-sm font-bold text-gray-800 mb-2">
              Price (€) *
            </label>
            <input
              id="price"
              type="text"
              value={price}
              onChange={handlePriceChange}
              placeholder="25.00 or 25,00"
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
            <p className="mt-1 text-sm text-gray-600">You can use either comma (25,00) or dot (25.00) as decimal separator</p>
          </div>

          <div className="flex gap-4 pt-4">
            <button
              type="button"
              onClick={handleClose}
              className="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-3 px-6 rounded-lg transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-600 hover:to-teal-700 disabled:from-gray-400 disabled:to-gray-500 text-white font-semibold py-3 px-6 rounded-lg transition-all duration-200 transform hover:scale-105 disabled:transform-none disabled:cursor-not-allowed"
            >
              {isSubmitting ? "Creating..." : "🚀 Create Offer"}
            </button>
          </div>
        </form>
      </div>
    </ModalWrapper>
  );
}