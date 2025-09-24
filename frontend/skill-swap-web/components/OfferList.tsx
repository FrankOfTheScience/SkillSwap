"use client";
import { useEffect, useState } from "react";
import api from "../services/api";
import { Offer } from "../types";
import { getCurrentUser } from "../services/auth";
import Link from "next/link";

export default function OfferList() {
  const [offers, setOffers] = useState<Offer[]>([]);
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    setUser(getCurrentUser());
    api.get("/offers").then(res => setOffers(res.data));
  }, []);

  const handleDelete = async (id: string) => {
    if (confirm("Are you sure you want to delete this offer?")) {
      await api.delete(`/offers/${id}`);
      setOffers(offers.filter(o => o.id !== id));
    }
  };

  return (
    <div className="grid gap-4">
      {offers.map(o => (
        <div key={o.id} className="p-4 border rounded shadow">
          <h3 className="text-xl font-bold">{o.title}</h3>
          <p>{o.description}</p>
          <p className="font-semibold">{o.price} €</p>
          {user?.role === "Admin" ? (
            <div className="mt-2 flex gap-2">
              <Link href={`/offers/${o.id}/edit`} className="bg-yellow-500 px-2 py-1 rounded text-white">Modify</Link>
              <button onClick={() => handleDelete(o.id)} className="bg-red-500 px-2 py-1 rounded text-white">Delete</button>
            </div>
          ) : (
            <button className="mt-2 bg-blue-500 text-white px-3 py-1 rounded">Book</button>
          )}
        </div>
      ))}
    </div>
  );
}
