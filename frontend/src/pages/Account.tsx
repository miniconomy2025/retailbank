import { useState } from "react";
import { Search, ArrowDownIcon, ArrowUpIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
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
import { getAccount, getAccountTransfers } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useParams } from "react-router-dom";
import type { Transfer } from "@/models/transfers";

export default function Account() {
  const { accountId } = useParams();
  const [searchTerm, setSearchTerm] = useState("");

  const {
    data: account,
    isLoading: isAccountLoading,
    error: accountError,
  } = useQuery<Account>({
    queryKey: [`account-${accountId}`],
    queryFn: () => getAccount(Number(accountId ?? 0)),
    refetchInterval: 15000,
  });

  const {
    data: transfers,
    isLoading: isTransferLoading,
    error: transferError,
  } = useQuery<Transfer[]>({
    queryKey: [`account-transfers-${accountId}`],
    queryFn: () => getAccountTransfers(Number(accountId ?? 0)),
    refetchInterval: 15000,
  });

  const filteredTransfers = transfers?.filter((transfer) => {
    const matchesSearch =
      transfer.transactionId.toString().includes(searchTerm.toLowerCase()) ||
      transfer.eventType
        .toString()
        .toLowerCase()
        .includes(searchTerm.toLowerCase());

    return matchesSearch;
  });

  const isDebit = (transfer: Transfer) =>
    transfer.debitAccountNumber === Number(accountId ?? 0);

  return (
    <PageWrapper
      loading={isAccountLoading || isTransferLoading}
      error={accountError || transferError}
    >
      <div className="flex flex-col gap-4">
        <div className="flex ">
          <div>
            <h1 className="text-3xl font-bold text-left">Account Details</h1>
            <p className="text-left">Account #{accountId}</p>
          </div>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Posted balance</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(account?.balancePosted)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Pending Balance</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(account?.balancePending)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Debit</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(account?.debitsPosted)}
              </div>
              <p className="text-s">
                Pending: {formatCurrency(account?.debitsPending)}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Credit</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(account?.creditsPending)}
              </div>
              <p className="text-s">
                Pending: {formatCurrency(account?.creditsPending)}
              </p>
            </CardContent>
          </Card>
        </div>
        <Card>
          <CardContent className="flex flex-col gap-4">
            <CardTitle className="text-left">Accounts Transfers</CardTitle>
            <div className="flex items-center gap-2">
              <div className="relative flex-1">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4" />
                <Input
                  placeholder="Search transfers by ID or type..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>From Account</TableHead>
                    <TableHead>To Account</TableHead>
                    <TableHead className="text-right">Amount</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredTransfers?.map((transfer) => (
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

            {filteredTransfers?.length === 0 && (
              <div className="text-center py-8 ">
                No transfers found matching your criteria.
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </PageWrapper>
  );
}
