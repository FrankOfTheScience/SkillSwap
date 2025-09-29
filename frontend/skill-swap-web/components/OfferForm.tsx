"use client";
import { useState, useEffect } from "react";
import api from "../services/api";
import { useRouter } from "next/navigation";

interface OfferFormProps {
  offerId?: string;
}

export default function OfferForm({ offerId }: OfferFormProps) {
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState(0);

  useEffect(() => {
    if (offerId) {
      api.get(`/api/offers/${offerId}`).then(res => {
        setTitle(res.data.title);
        setDescription(res.data.description);
        setPrice(res.data.price);
      });
    }
  }, [offerId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (offerId) {
      await api.put(`/api/offers/${offerId}`, { title, description, price });
    } else {
      await api.post("/api/offers", { title, description, price });
    }
    router.push("/offers");
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-2 max-w-md mx-auto mt-8">
      <input value={title} onChange={e => setTitle(e.target.value)} placeholder="Title" className="border p-2 rounded"/>
      <textarea value={description} onChange={e => setDescription(e.target.value)} placeholder="Description" className="border p-2 rounded"/>
      <input type="number" value={price} onChange={e => setPrice(Number(e.target.value))} placeholder="Price" className="border p-2 rounded"/>
      <button type="submit" className="bg-green-500 text-white p-2 rounded">
        {offerId ? "Update" : "Create"}
      </button>
    </form>
  );
}
