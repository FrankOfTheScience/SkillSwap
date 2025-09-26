"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function CreateOfferPage() {
  const router = useRouter();

  useEffect(() => {
    // Redirect to homepage - the homepage will handle opening the create offer modal via URL params
    router.replace("/?showModal=createOffer");
  }, [router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
        <p className="text-gray-600">Opening create offer...</p>
      </div>
    </div>
  );
}