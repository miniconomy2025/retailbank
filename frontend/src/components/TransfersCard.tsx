import { formatCurrency } from "@/utils/formatter";
import type { TransferPage } from "@/models/transfers";

export default function TransfersCard({
  transferPage,
}: {
  transferPage: TransferPage;
}) {
  return (
    <div className="overflow-x-auto h-[90%] overflow-y-auto border shadow-sm">
      <table className="min-w-full table-auto text-left text-sm">
        <thead className=" bg-slate-100 text-slate-800">
          <tr>
            <th className="px-2 py-2">DR Account</th>
            <th className="px-2 py-2">CR Account</th>
            <th className="px-2 py-2">Amount(Đ)</th>
          </tr>
        </thead>
        <tbody>
          {transferPage?.items.map((item, index) => (
            <tr
              key={index}
              className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
            >
              <td className="px-2 py-2">{item.debitAccountId}</td>
              <td className="px-2 py-2">{item.creditAccountId}</td>
              <td
                className={`px-2 py-2 font-semibold ${
                  item.debitAccountId != "1000"
                    ? item.creditAccountId === "1000"
                      ? "text-red-600"
                      : "text-yellow-400"
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
