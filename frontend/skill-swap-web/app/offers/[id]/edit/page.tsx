import OfferForm from "../../../../components/OfferForm";
import Navbar from "../../../../components/Navbar";

interface EditProps { params: { id: string } }

export default function EditOfferPage({ params }: EditProps) {
  return (
    <>
      <Navbar />
      <OfferForm offerId={params.id} />
    </>
  );
}
