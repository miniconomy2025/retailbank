import { useParams } from "react-router-dom";
function Account() {
  const { accountId } = useParams();
  return <h1>Account Page for {accountId}</h1>;
}
export default Account;