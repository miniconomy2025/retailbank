import { useParams } from "react-router-dom";
function Transfer() {
  const { transferId } = useParams();
  return (
    <h1 className="h-full w-full flex items-center justify-center text-4xl">
      Transfer page for {transferId}
    </h1>
  );
}
export default Transfer;
