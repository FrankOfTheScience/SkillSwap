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
  const [durationInMinutes, setDurationInMinutes] = useState(60);
  const [location, setLocation] = useState("");
  const [isOnline, setIsOnline] = useState(true);
  const [requirements, setRequirements] = useState("");
  const [category, setCategory] = useState("");
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

    console.log('Creating offer with data:', {
      title,
      description,
      price: numericPrice,
      durationInMinutes,
      location: location || null,
      isOnline,
      requirements: requirements || null,
      category: category || null
    });

    try {
      const response = await api.post("/api/offers", {
        title,
        description,
        price: numericPrice,
        durationInMinutes,
        location: location || null,
        isOnline,
        requirements: requirements || null,
        category: category || null
      });
      console.log('Offer created successfully:', response);
      onSuccess();
      // Reset form
      resetForm();
    } catch (err: unknown) {
      console.error("Create offer error details:", err);
      
      // Enhanced error logging for debugging
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosError = err as { 
          response?: { 
            data?: unknown; 
            status?: number; 
            statusText?: string;
          }; 
          message?: string; 
          request?: unknown;
          config?: unknown;
        };
        
        console.error("Response status:", axiosError.response?.status);
        console.error("Response data:", axiosError.response?.data);
        console.error("Response statusText:", axiosError.response?.statusText);
        console.error("Request config:", axiosError.config);
        
        // Try to extract meaningful error message
        let errorMessage = "Failed to create offer";
        
        if (axiosError.response?.data) {
          const responseData = axiosError.response.data as Record<string, unknown>;
          if (typeof responseData === 'string') {
            errorMessage = responseData;
          } else if (typeof responseData?.message === 'string') {
            errorMessage = responseData.message;
          } else if (typeof responseData?.error === 'string') {
            errorMessage = responseData.error;
          } else if (responseData?.errors) {
            // Handle validation errors
            if (Array.isArray(responseData.errors)) {
              errorMessage = responseData.errors.map((e: {errorMessage?: string; message?: string} | string) => 
                typeof e === 'string' ? e : e.errorMessage || e.message || 'Validation error'
              ).join(', ');
            } else {
              errorMessage = "Validation errors occurred";
            }
          } else if (typeof responseData?.title === 'string') {
            errorMessage = responseData.title;
          }
        } else if (axiosError.response?.status === 401) {
          errorMessage = "Please log in to create an offer";
        } else if (axiosError.response?.status === 403) {
          errorMessage = "You don't have permission to create offers";
        } else if (axiosError.response?.status && axiosError.response.status >= 500) {
          errorMessage = "Server error. Please try again later";
        } else if (axiosError.message) {
          errorMessage = axiosError.message;
        }
        
        setError(errorMessage);
      } else if (err instanceof Error) {
        console.error("Error message:", err.message);
        console.error("Error stack:", err.stack);
        setError(err.message);
      } else {
        console.error("Unknown error type:", typeof err, err);
        setError("An unexpected error occurred");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetForm = () => {
    setTitle("");
    setDescription("");
    setPrice("");
    setDurationInMinutes(60);
    setLocation("");
    setIsOnline(true);
    setRequirements("");
    setCategory("");
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
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors bg-white text-gray-900"
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
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors bg-white text-gray-900"
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
                  className="mr-2 text-emerald-600"
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
                  className="mr-2 text-emerald-600"
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
                className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors bg-white text-gray-900 placeholder-gray-500"
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
              className="w-full px-4 py-3 border-2 border-gray-400 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors resize-vertical bg-white text-gray-900 placeholder-gray-500"
              disabled={isSubmitting}
            />
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