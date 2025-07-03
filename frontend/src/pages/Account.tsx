import { ArrowDownIcon, ArrowUpIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { type Account } from "@/models/accounts";
import { useQuery } from "@tanstack/react-query";
import { getAccountTransfers } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useParams } from "react-router-dom";
import type { Transfer } from "@/models/transfers";

export default function Account() {
  const { accountId: accountIdString } = useParams();
  const accountId = Number(accountIdString ?? 0);

  const {
    data: transfers,
    isLoading: isTransferLoading,
    error: transferError,
  } = useQuery<Transfer[]>({
    queryKey: [`account-transfers-${accountId}`],
    queryFn: () => getAccountTransfers(accountId),
  });

  const isDebit = (transfer: Transfer) =>
    transfer.debitAccountNumber === accountId;

  return (
    <PageWrapper loading={isTransferLoading} error={transferError}>
      <div className="h-full flex flex-col gap-4">
        <div className="flex">
          <div>
            <h1 className="text-3xl font-bold text-left">Accounts Transfers</h1>
            <p className="text-left">Account {accountId}</p>
          </div>
        </div>
        <div className="rounded-md border overflow-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>ID</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>From</TableHead>
                <TableHead>To</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                <TableHead className="text-center">Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {transfers?.map((transfer) => (
                <TableRow key={transfer.transactionId}>
                  <TableCell className="text-left">
                    {transfer.transactionId}
                  </TableCell>
                  <TableCell className="text-left">
                    <div className="flex items-center gap-1">
                      {isDebit(transfer) ? (
                        <>
                          <ArrowDownIcon className="h-4 w-4 text-red-500" />
                          <span className="text-red-600">Debit</span>
                        </>
                      ) : (
                        <>
                          <ArrowUpIcon className="h-4 w-4 text-green-500" />
                          <span className="text-green-600">Credit</span>
                        </>
                      )}
                    </div>
                  </TableCell>
                  <TableCell className="text-left">
                    {transfer.debitAccountNumber}
                  </TableCell>
                  <TableCell className="text-left">
                    {transfer.creditAccountNumber}
                  </TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(transfer.amount)}
                  </TableCell>
                  <TableCell className="text-center">
                    <Badge>{transfer.eventType}</Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {transfers?.length === 0 && (
          <div className="text-center py-8 ">
            No transfers found matching your criteria.
          </div>
        )}
      </div>
    </PageWrapper>
  );
}
