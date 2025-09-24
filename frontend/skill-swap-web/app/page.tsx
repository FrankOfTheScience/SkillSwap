import OfferList from "../components/OfferList";

export default function Home() {
  return (
    <main className="p-8">
      <h1 className="text-3xl font-bold mb-6">SkillSwap Offers</h1>
      <OfferList />
    </main>
  );
}
