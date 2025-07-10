import { ArrowDownIcon, ArrowUpIcon, ExternalLink } from "lucide-react";
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
import { getAccount, getAccountLoans, getAccountTransfers } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useNavigate, useParams } from "react-router-dom";
import type { Transfer, TransferPage } from "@/models/transfers";
import { useEffect, useRef } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";

export default function Account() {
  const navigate = useNavigate();
  const { accountId: accountIdString } = useParams();
  const accountId = accountIdString ?? "0";

  const {
    data: account,
    isLoading: isAccountLoading,
    error: accountError,
  } = useQuery<Account>({
    queryKey: [`account-${accountId}`],
    queryFn: () => getAccount(accountId),
    retry: false,
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
    retry: false,
  });

  const {
    data: loanAccounts,
    isLoading: isLoanAccountsLoading,
    error: loanAccountsError,
  } = useQuery<Account[]>({
    queryKey: [`account-${accountId}-loans`],
    queryFn: () => getAccountLoans(accountId),
    retry: false,
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
    transfer.debitAccountId === accountId;

  return (
    <PageWrapper
      loading={isAccountLoading || isTransfersLoading || isLoanAccountsLoading}
      error={accountError || transfersError || loanAccountsError}
    >
      <div className="h-full flex flex-col gap-4">
        <div>
          <h1 className="text-3xl font-bold text-left">Accounts Transfers</h1>
          <p className="text-left">Account {accountId}</p>
        </div>
        <AccountCardRow account={account} />

        {/* Loans Section (shown only for non-loan accounts) */}
        { account && account.accountType !== 'Loan' ? (
          <section className="rounded-md border overflow-auto">
            {loanAccounts?.length ? <h2 className="text-2xl font-bold px-4 py-2 justify-self-start">Loans</h2> : null}
            <Accordion type="multiple">
              {loanAccounts?.length && loanAccounts.length > 0 ? (<>{
                loanAccounts.map((loanAccount) => (
                  <AccordionItem key={loanAccount.id} value={loanAccount.id}>
                    <AccordionTrigger className="px-4">
                      Account {loanAccount.id} {loanAccount.closed ? "(Closed)" : "(Open)"}
                    </AccordionTrigger>
                    <AccordionContent className="p-4">
                      <span 
                        className="flex items-center gap-2 mb-2 cursor-pointer hover:underline w-fit"
                        onClick={() => navigate(`/accounts/${loanAccount.id}`)}
                      >
                        <ExternalLink
                          className="h-4 w-4"
                        />
                        <span className="text-sm">
                          View Account
                        </span>
                      </span>
                      <AccountCardRow account={loanAccount} />
                    </AccordionContent>
                  </AccordionItem>
                ))
              }</>) : (
                <div className="text-center py-8">
                  No loans found for this account
                </div>
              )}
            </Accordion>
          </section>
        ) : null }


        {/* Transfers Section */}
        <div className="rounded-md border overflow-auto">
          {transfers?.length ? <h2 className="text-2xl font-bold px-4 py-2 justify-self-start">Transfers</h2> : null}
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
                    <TableRow key={transfer.transferId + transfer.timestamp}>
                      <TableCell className="text-left">
                        {transfer.transferId}
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
                        {transfer.debitAccountId}
                      </TableCell>
                      <TableCell className="text-left">
                        {transfer.creditAccountId}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(transfer.amount)}
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge>{transfer.transferType}</Badge>
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

const AccountCardRow = ({ account }: { account: Account | undefined }) => (
  <>
    { account ? (
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex">
            <CardTitle className="font-medium">Posted balance</CardTitle>
          </CardHeader>
      <CardContent>
        <div className="text-xl font-bold">
          {formatCurrency(account?.balancePosted ?? 0)}
        </div>
      </CardContent>
    </Card>
    <Card>
      <CardHeader className="flex">
        <CardTitle className="font-medium">Pending Balance</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-xl font-bold">
          {formatCurrency(account?.balancePending ?? 0)}
        </div>
      </CardContent>
    </Card>
    <Card>
      <CardHeader className="flex">
        <CardTitle className="font-medium">Total Debit</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-xl font-bold">
          {formatCurrency(account?.debitsPosted ?? 0)}
        </div>
      </CardContent>
    </Card>
    <Card>
      <CardHeader className="flex">
        <CardTitle className="font-medium">Total Credit</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-xl font-bold">
          {formatCurrency(account?.creditsPending ?? 0)}
        </div>
      </CardContent>
    </Card>
  </div> 
    ) : (
      <div className="text-center py-8">Account not found</div>
    )}
  </>
);