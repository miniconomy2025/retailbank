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
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { getAccount, getAccountTransfers } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useParams } from "react-router-dom";
import type { Transfer, TransferPage } from "@/models/transfers";
import { useEffect, useRef } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function Account() {
  const { accountId: accountIdString } = useParams();
  const accountId = Number(accountIdString ?? 0);

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
    data,
    isLoading: isTransfersLoading,
    error: transfersError,
    fetchNextPage,
    hasNextPage,
  } = useInfiniteQuery<TransferPage>({
    queryKey: ["account-transfers", accountId],
    queryFn: ({ pageParam }) =>
      getAccountTransfers(accountId, pageParam as string | undefined),
    getNextPageParam: (lastPage) => lastPage.next || undefined,
    initialPageParam: undefined,
  });

  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!bottomRef.current || !hasNextPage) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          fetchNextPage();
        }
      },
      { threshold: 1 }
    );

    observer.observe(bottomRef.current);

    return () => observer.disconnect();
  }, [hasNextPage, fetchNextPage]);

  const transfers = data?.pages.flatMap((page) => page.items) ?? [];

  const isDebit = (transfer: Transfer) =>
    transfer.debitAccountNumber === accountId;

  return (
    <PageWrapper
      loading={isAccountLoading || isTransfersLoading}
      error={accountError || transfersError}
    >
      <div className="h-full flex flex-col gap-4">
        <div className="flex">
          <div>
            <h1 className="text-3xl font-bold text-left">Accounts Transfers</h1>
            <p className="text-left">Account {accountId}</p>
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
            </CardContent>
          </Card>
        </div>
        <div className="rounded-md border overflow-auto">
          {transfers?.length === 0 ? (
            <div className="text-center py-8 ">
              No transfers found for this account
            </div>
          ) : (
            <>
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
                    <TableRow key={transfer.transactionId + transfer.timestamp}>
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
              <div ref={bottomRef} className="h-px" />
            </>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}
