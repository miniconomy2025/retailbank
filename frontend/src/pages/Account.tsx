import { useParams } from "react-router-dom";
function Account() {
  const { accountId } = useParams();
  return <main className="h-full flex items-center justify-center"><h1>Account Page for {accountId}</h1></main>;
}
export default Account;