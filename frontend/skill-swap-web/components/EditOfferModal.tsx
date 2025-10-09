"use client";
import { useState, useEffect } from "react";
import api from "../services/api";
import ModalWrapper from "./ModalWrapper";

interface EditOfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  offerId: string | null;
  onSuccess?: () => void;
}

export default function EditOfferModal({ isOpen, onClose, offerId, onSuccess }: EditOfferModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [durationInMinutes, setDurationInMinutes] = useState(60);
  const [location, setLocation] = useState("");
  const [isOnline, setIsOnline] = useState(true);
  const [requirements, setRequirements] = useState("");
  const [category, setCategory] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isOpen || !offerId) {
      // Reset form when modal closes
      setTitle("");
      setDescription("");
      setPrice("");
      setDurationInMinutes(60);
      setLocation("");
      setIsOnline(true);
      setRequirements("");
      setCategory("");
      setError("");
      setLoading(true);
      return;
    }

    const fetchOffer = async () => {
      try {
        const response = await api.get(`/api/offers/${offerId}`);
        const offer = response.data;
        setTitle(offer.title);
        setDescription(offer.description);
        setPrice(offer.price.toString());
        setDurationInMinutes(offer.durationInMinutes || 60);
        setLocation(offer.location || "");
        setIsOnline(offer.isOnline ?? true);
        setRequirements(offer.requirements || "");
        setCategory(offer.category || "");
      } catch (err: unknown) {
        console.error("Fetch offer error:", err);
        setError("Failed to load offer details");
      } finally {
        setLoading(false);
      }
    };

    fetchOffer();
  }, [isOpen, offerId]);

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
      await api.put(`/api/offers/${offerId}`, {
        title,
        description,
        price: numericPrice,
        durationInMinutes,
        location: location || null,
        isOnline,
        requirements: requirements || null,
        category: category || null
      });
      
      if (onSuccess) {
        onSuccess();
      }
      onClose();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string; error?: string }; status?: number }; message?: string; config?: unknown };
      console.error("Update offer error:", {
        message: error.message,
        response: error.response,
        status: error.response?.status,
        data: error.response?.data,
        config: error.config
      });
      setError(error.response?.data?.message || error.response?.data?.error || error.message || "Failed to update offer");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    if (!isSubmitting) {
      onClose();
    }
  };

  if (!isOpen) return null;

  if (loading) {
    return (
      <ModalWrapper title="Update Offer" isOpen={isOpen} onClose={handleClose}>
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading offer details...</p>
        </div>
      </ModalWrapper>
    );
  }

  return (
    <ModalWrapper title="Update Offer" isOpen={isOpen} onClose={handleClose}>
      <div className="bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 rounded-xl border border-blue-100 shadow-lg p-6">
        <div className="text-center mb-6">
          <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-indigo-600 rounded-full flex items-center justify-center mx-auto mb-4">
            <span className="text-white text-2xl">✏️</span>
          </div>
          <p className="text-gray-600">
            Make changes to your skill offering
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
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
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
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors resize-vertical bg-white text-gray-900 placeholder-gray-500"
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
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
            <p className="mt-1 text-sm text-gray-600">You can use either comma (25,00) or dot (25.00) as decimal separator</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="duration" className="block text-sm font-bold text-gray-800 mb-2">
                Duration (minutes) *
              </label>
              <input
                id="duration"
                type="number"
                min="15"
                step="15"
                value={durationInMinutes}
                onChange={(e) => setDurationInMinutes(parseInt(e.target.value) || 60)}
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors bg-white text-gray-900"
                disabled={isSubmitting}
              />
            </div>

            <div>
              <label htmlFor="category" className="block text-sm font-bold text-gray-800 mb-2">
                Category
              </label>
              <select
                id="category"
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors bg-white text-gray-900"
                disabled={isSubmitting}
              >
                <option value="">Select a category</option>
                <option value="Technology">Technology</option>
                <option value="Education">Education</option>
                <option value="Business">Business</option>
                <option value="Creative">Creative</option>
                <option value="Health">Health</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-800 mb-2">
              Delivery Method *
            </label>
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="radio"
                  name="isOnline"
                  checked={isOnline}
                  onChange={() => setIsOnline(true)}
                  className="mr-2 text-blue-600"
                  disabled={isSubmitting}
                />
                <span className="text-gray-700">Online</span>
              </label>
              <label className="flex items-center">
                <input
                  type="radio"
                  name="isOnline"
                  checked={!isOnline}
                  onChange={() => setIsOnline(false)}
                  className="mr-2 text-blue-600"
                  disabled={isSubmitting}
                />
                <span className="text-gray-700">In-person</span>
              </label>
            </div>
          </div>

          {!isOnline && (
            <div>
              <label htmlFor="location" className="block text-sm font-bold text-gray-800 mb-2">
                Location *
              </label>
              <input
                id="location"
                type="text"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                placeholder="e.g., Downtown Office, Central Library, etc."
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
                disabled={isSubmitting}
              />
            </div>
          )}

          <div>
            <label htmlFor="requirements" className="block text-sm font-bold text-gray-800 mb-2">
              Requirements
            </label>
            <textarea
              id="requirements"
              value={requirements}
              onChange={(e) => setRequirements(e.target.value)}
              placeholder="Any prerequisites, materials needed, or preparation required..."
              rows={3}
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors resize-vertical bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
          </div>

          <div className="flex gap-4 pt-4">
            <button
              type="button"
              onClick={handleClose}
              disabled={isSubmitting}
              className="flex-1 bg-gray-200 hover:bg-gray-300 disabled:bg-gray-100 text-gray-700 font-medium py-3 px-6 rounded-lg transition-colors disabled:cursor-not-allowed"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 disabled:from-gray-400 disabled:to-gray-500 text-white font-semibold py-3 px-6 rounded-lg transition-all duration-200 transform hover:scale-105 disabled:transform-none disabled:cursor-not-allowed"
            >
              {isSubmitting ? "Updating..." : "✏️ Update Offer"}
            </button>
          </div>
        </form>
      </div>
    </ModalWrapper>
  );
}