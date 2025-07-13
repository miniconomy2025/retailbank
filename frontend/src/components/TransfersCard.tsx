import { formatCurrency } from "@/utils/formatter";
import type { TransferPage } from "@/models/transfers";
import { Link } from "react-router-dom";
import { accountLink, accountName } from "@/models/accounts";

export default function TransfersCard({
  transferPage,
}: {
  transferPage: TransferPage;
}) {
  return (
    <div className="overflow-x-auto h-[92%] overflow-y-auto border shadow-sm">
      <table className="min-w-full table-auto text-left text-sm transfer-card">
        <thead className=" bg-slate-100 text-slate-800">
          <tr>
            <th className="px-2 py-2">Debit Account</th>
            <th className="px-2 py-2">Credit Account</th>
            <th className="px-2 py-2">Amount (Đ)</th>
          </tr>
        </thead>
        <tbody>
          {transferPage?.items.map((item, index) => (
            <tr
              key={index}
              className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
            >
              <td className="px-2 py-2">
                <Link to={accountLink(item.debitAccountId, item.transferId)}>
                  <span className="font-mono">{item.debitAccountId}</span>
                  <small className="ml-2 text-gray-500">{accountName(item.debitAccountId)}</small>                </Link>
              </td>
              <td className="px-2 py-2">
                <Link to={accountLink(item.creditAccountId, item.transferId)}>
                  <span className="font-mono">{item.creditAccountId}</span>
                  <small className="ml-2 text-gray-500">{accountName(item.creditAccountId)}</small>                </Link>
              </td>
              <td
                className={`px-2 py-2 font-mono ${
                  item.debitAccountId != "1000"
                    ? item.creditAccountId === "1000"
                      ? "text-red-600"
                      : "text-gray-600"
                    : "text-green-600"
                }`}
              >
                {formatCurrency(item.amount).substring(2)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
