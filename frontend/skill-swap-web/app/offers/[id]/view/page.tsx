import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import api from "../../../../services/api";
import Navbar from "../../../../components/Navbar";
import { Offer } from "../../../../types";
import { getCurrentUser } from "../../../../services/auth";

export default function ViewOfferPage() {
  const params = useParams();
  const router = useRouter();
  const [offer, setOffer] = useState<Offer | null>(null);
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    setUser(getCurrentUser());
    api.get(`/offers/${params.id}`).then(res => setOffer(res.data));
  }, [params.id]);

  const handleDelete = async () => {
    if (confirm("Sei sicuro di voler eliminare questa offerta?")) {
      await api.delete(`/offers/${params.id}`);
      router.push("/offers");
    }
  };

  if (!offer) return <p>Loading...</p>;

  return (
    <>
      <Navbar />
      <div className="max-w-md mx-auto mt-8 p-4 border rounded">
        <h1 className="text-2xl font-bold mb-2">{offer.title}</h1>
        <p>{offer.description}</p>
        <p className="font-semibold mb-4">{offer.price} €</p>
        <div className="flex gap-2">
          {user?.role === "Admin" ? (
            <>
              <button onClick={() => router.push(`/offers/${offer.id}/edit`)} className="bg-yellow-500 px-3 py-1 rounded text-white">Modify</button>
              <button onClick={handleDelete} className="bg-red-500 px-3 py-1 rounded text-white">Delete</button>
            </>
          ) : (
            <button className="bg-blue-500 px-3 py-1 rounded text-white">Book</button>
          )}
        </div>
      </div>
    </>
  );
}
