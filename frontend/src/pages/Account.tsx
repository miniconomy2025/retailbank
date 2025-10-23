import { AccountNames, type Account } from "@/models/accounts";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { getAccount, getAccountLoans, getAccountTransfers } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useParams } from "react-router-dom";
import type { TransferPage } from "@/models/transfers";
import { useEffect, useRef } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import TransferTable from "@/components/TransferTable";
import { AccountTable } from "@/components/AccountTable";
import { BanknoteArrowDown, BanknoteArrowUp, BanknoteIcon } from "lucide-react";
import { formatDate } from "@/lib/utils";

export default function Account() {
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

  const accountName = accountId in AccountNames
    ? AccountNames[accountId as keyof typeof AccountNames]
    : '';

  return (
    <PageWrapper
      loading={isAccountLoading || isTransfersLoading || isLoanAccountsLoading}
      error={accountError || transfersError || loanAccountsError}
    >
      <div className="h-full flex flex-col gap-4">
        <div className="flex">
          <div className="flex-1">
            <h1 className="text-3xl font-thin text-left">
              <span className="font-mono">{accountId}</span> {accountName && <span className="text-2xl">{accountName}</span>}
            </h1>
            <h2 className="text-xl font-thin text-left text-gray-500">
              {account?.accountType} Account
            </h2>
          </div>
          <div className="text-right">
            {
              account?.createdAt &&
              <>
                <p>Created At</p>
                <p className="text-gray-500 text-sm">{formatDate(new Date(account.createdAt))}</p>
              </>
            }
          </div>
        </div>

        <div className="flex gap-4 flex-col lg:flex-row align-center">
          <div>
            <h2 className="text-left text-xl font-thin px-2 py-2">Account Summary</h2>
            {
              account
              ? <AccountCardRow account={account}/>
              : <div className="text-center py-8">Account not found</div>
            }
          </div>
          {/* Loans Section */}
          { account && account.accountType === 'Transactional' && loanAccounts ? (
            <div className="h-full flex-1">
              <div className="rounded-md border overflow-auto min-w-80 h-44 lg:h-80 xl:h-44">
                <h2 className="text-left text-xl font-thin px-4 py-2">Loans</h2>
                <hr />
                {
                  loanAccounts.length > 0
                  ? <AccountTable accounts={loanAccounts} summary={true}/>
                  : <div className="text-center py-8 ">
                    No loans found for this account
                  </div>
                }
              </div>
            </div>
          ) : null }
        </div>

        {/* Transfers Section */}
        {
          transfers &&
          <>
            <div className="rounded-md border overflow-auto min-h-44">
              <h2 className="text-left text-xl font-thin px-4 py-2 justify-self-start">Transfers</h2>
              <hr />
              {transfers?.length === 0 ? (
                <div className="text-center py-8 ">
                  No transfers found for this account
                </div>
              ) : (
                <>
                  <TransferTable transfers={transfers} accountId={account?.id}/>
                  <div ref={bottomRef} className="h-px" />
                </>
              )}
            </div>
          </>
        }
      </div>
    </PageWrapper>
  );
}

const AccountCardRow = ({ account }: { account: Account }) => (
  <div className={`grid content-center gap-4 grid-cols-1 sm:grid-cols-2 md:grid-cols-3 w-full ${account.accountType !== 'Transactional' ? "lg:grid-cols-3" : "lg:grid-cols-2 xl:grid-cols-3"}`}>
    <Card>
      <CardHeader className="flex">
        <BanknoteArrowUp className="w-6 h-6"/>
        <CardTitle className="text-left text-xl font-light flex-1 leading-6">Debit</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex justify-center text-lg">
          <div className="text-left">
            <small>Posted:</small> <span className="ml-2 font-mono">{formatCurrency(account.posted.debits)}</span>
            <br/>
            <small>Pending:</small> <span className="ml-2 font-mono">{formatCurrency(account.pending.debits)}</span>          </div>
        </div>
      </CardContent>
    </Card>
    <Card>
      <CardHeader className="flex">
        <BanknoteArrowDown className="w-6 h-6"/>
        <CardTitle className="text-left text-xl font-light flex-1 leading-6">Credit</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex justify-center text-lg">
          <div className="text-left">
            <small>Posted:</small> <span className="ml-2 font-mono">{formatCurrency(account.posted.credits)}</span>
            <br/>
            <small>Pending:</small> <span className="ml-2 font-mono">{formatCurrency(account.pending.credits)}</span>          </div>
        </div>
      </CardContent>
    </Card>
    <Card>
      <CardHeader className="flex">
        <BanknoteIcon className="w-6 h-6"/>
        <CardTitle className="text-left text-xl font-light flex-1 leading-6">Balance</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex justify-center text-lg">
          <div className="text-left">
            <small>Posted:</small> <span className="ml-2 font-mono">{formatCurrency(account.posted.balance)}</span>
            <br/>
            <small>Pending:</small> <span className="ml-2 font-mono">{formatCurrency(account.pending.balance)}</span>          </div>
        </div>
      </CardContent>
    </Card>
  </div>
);