"use client";
import { useRouter } from "next/navigation";
import ModalWrapper from "../../../components/ModalWrapper";

export default function BookingCancelPage() {
  const router = useRouter();

  const handleBackToOffers = () => {
    router.push("/");
  };

  const handleTryAgain = () => {
    router.back();
  };

  return (
    <ModalWrapper title="Booking Cancelled">
      <div className="text-center py-12">
        <div className="text-6xl mb-6">😔</div>
        <h2 className="text-2xl font-bold text-gray-800 mb-4">Booking Cancelled</h2>
        <p className="text-gray-600 mb-8">
          Your booking was cancelled and no payment was processed. 
          You can try booking again or browse other offers.
        </p>

        {/* Helpful Information */}
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="font-semibold text-yellow-900 mb-2">Why was my booking cancelled?</h3>
          <ul className="text-sm text-yellow-800 space-y-1">
            <li>• You closed the payment window</li>
            <li>• Payment was declined by your bank</li>
            <li>• Session expired due to inactivity</li>
            <li>• You chose to cancel the payment</li>
          </ul>
        </div>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="font-semibold text-blue-900 mb-2">Need help?</h3>
          <p className="text-sm text-blue-800">
            If you experienced any technical issues or have questions about the booking process, 
            please contact our support team.
          </p>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-4 justify-center">
          <button
            onClick={handleTryAgain}
            className="bg-emerald-600 hover:bg-emerald-700 text-white px-8 py-3 rounded-lg font-semibold transition-colors flex items-center gap-2"
          >
            🔄 Try Again
          </button>
          <button
            onClick={handleBackToOffers}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold transition-colors"
          >
            Browse Other Offers
          </button>
        </div>

        {/* Contact Support */}
        <div className="mt-8 pt-6 border-t">
          <p className="text-sm text-gray-500">
            Having trouble? {" "}
            <button className="text-blue-600 hover:text-blue-700 underline">
              Contact Support
            </button>
          </p>
        </div>
      </div>
    </ModalWrapper>
  );
}