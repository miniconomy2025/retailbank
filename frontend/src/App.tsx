import "./App.css";
import { Routes, Route } from 'react-router-dom'
import Dashboard from "./pages/Dashboard";
import Account from "./pages/Account";
import NotFound from "./pages/NotFound";


function App() {
  return (
    <Routes>
      <Route path="/" element={<Dashboard />} />
      <Route path="/account/:accountId" element={<Account />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  )
}

export default App;