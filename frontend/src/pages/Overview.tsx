import { getAccount } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Account } from "@/models/accounts";
import { formatCurrency } from "@/utils/formatter";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { Activity, BanknoteArrowUp } from "lucide-react";
import { type TransferPage } from "@/models/transfers";
import { getTransfers } from "@/api/transfers";
import ChartsCard from "@/components/ChartsCard";
import TransfersCard from "@/components/TransfersCard";

export default function Overview() {
  const { data: retailAcc, isLoading: retailLoading } = useQuery<Account>({
    queryKey: [`account-${1000}`],
    queryFn: () => getAccount("1000"),
    retry: false,
    refetchInterval: 5000,
  });

  const { data: interestIncomeAcc, isLoading: interestIncomeLoading } =
    useQuery<Account>({
      queryKey: [`account-${1002}`],
      queryFn: () => getAccount("1002"),
      retry: false,
      refetchInterval: 5000,
    });

  const { data: loanControlAcc, isLoading: loanControlLoading } =
    useQuery<Account>({
      queryKey: [`account-${1003}`],
      queryFn: () => getAccount("1003"),
      retry: false,
      refetchInterval: 5000,
    });

  const { data: commercialBankAcc, isLoading: commercialBankLoading } =
    useQuery<Account>({
      queryKey: [`account-${2000}`],
      queryFn: () => getAccount("2000"),
      retry: false,
      refetchInterval: 5000,
    });

  const {
    data: transferPage,
    isLoading: transferPageLoading,
    error: transferPageError,
  } = useQuery<TransferPage>({
    queryKey: ["OverviewTransfers"],
    queryFn: () => getTransfers(undefined, 30),
    refetchInterval: 5000,
  });

  return (
    <PageWrapper
      loading={
        ((retailLoading && retailAcc) ||
          (interestIncomeLoading && interestIncomeAcc) ||
          (loanControlLoading && loanControlAcc) ||
          (commercialBankLoading && commercialBankAcc) ||
          (transferPageLoading && transferPage)) ??
        false
      }
      error={transferPageError}
    >
      <main className="w-full h-full flex flex-row gap-2">
        <div className="scroll-auto min-h-[100%] w-[70%]">
          <div className=" flex flex-col gap-4">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              {retailAcc && (
                <AccountCard title="Retail Bank" account={retailAcc} />
              )}
              {commercialBankAcc && (
                <AccountCard
                  title="Commercial Bank"
                  account={commercialBankAcc}
                />
              )}
              {loanControlAcc && (
                <AccountCard title="Loan Control" account={loanControlAcc} />
              )}
              {interestIncomeAcc && (
                <AccountCard
                  title="Interest Income"
                  account={interestIncomeAcc}
                />
              )}
            </div>
          </div>
          <ChartsCard />
        </div>
        <div className="flex-col h-full flex-1 p-4 rounded-2xl shadow-lg border-slate-100 border-2">
          <div className="flex flex-row items-center gap-2 mb-4">
            <Activity />
            <h3 className="text-lg font-semibold">Recent Transfers</h3>
          </div>
          <TransfersCard transferPage={transferPage ?? { items: [] }} />
        </div>
      </main>
    </PageWrapper>
  );
}

function AccountCard({ title, account }: { title: string; account: Account }) {
  const navigate = useNavigate();
  const displayBalance =
    account.id == "1002" ? account.balancePosted * -1 : account.balancePosted;

  return (
    <Card
      onClick={() => account && navigate(`/accounts/${account.id}`)}
      className="cursor-pointer hover:shadow-lg hover:bg-gray-100"
    >
      <CardHeader className="flex">
        <BanknoteArrowUp />
        <CardTitle className="text-xl font-light">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div
          className={
            displayBalance <= 0
              ? "flex justify-start font-extrabold text-lg text-bol text-red-500"
              : "flex justify-start font-extrabold text-lg text-bol text-green-500"
          }
        >
          {formatCurrency(displayBalance)}
        </div>
      </CardContent>
    </Card>
  );
}
