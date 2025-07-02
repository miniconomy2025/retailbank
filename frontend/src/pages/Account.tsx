import { useParams } from "react-router-dom";
function Account() {
  const { accountId } = useParams();
  return  <h1 className="h-full w-full flex items-center justify-center text-4xl">Account page for {accountId}</h1>;
}
export default Account;
