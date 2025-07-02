import "./App.css";
import { Routes, Route } from "react-router-dom";
import Overview from "./pages/Overview";
import Account from "./pages/Account";
import NotFound from "./pages/NotFound";
import Navbar from "./components/Navbar";
import Accounts from "./pages/Accounts";
import Transfers from "./pages/Transfers";
import Transfer from "./pages/Transfer";

function App() {
  return (
    <div className="flex flex-col h-screen w-screen">
      <Navbar />
      <main className="flex-1 overflow-auto p-4 box-border items-center justify-center">
        <Routes>
          <Route path="/" element={<Overview />} />
          <Route path="/accounts" element={<Accounts />} />
          <Route path="/account/:accountId" element={<Account />} />
          <Route path="/transfers" element={<Transfers />} />
          <Route path="/transfers/:transferId" element={<Transfer />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
