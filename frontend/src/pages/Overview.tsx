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
import Charts from "@/components/Charts";
import TransfersCard from "@/components/TransfersCard";
import { cn } from "@/lib/utils";

export default function Overview() {
  const { data: retailAcc, isLoading: retailLoading } = useQuery<Account>({
    queryKey: [`account-${1000}`],
    queryFn: () => getAccount("1000"),
    retry: false,
    refetchInterval: 10000,
  });

  const { data: interestIncomeAcc, isLoading: interestIncomeLoading } =
    useQuery<Account>({
      queryKey: [`account-${1002}`],
      queryFn: () => getAccount("1002"),
      retry: false,
      refetchInterval: 10000,
    });

  const { data: feeIncomeAcc, isLoading: feeIncomeLoading } =
    useQuery<Account>({
      queryKey: [`account-${1005}`],
      queryFn: () => getAccount("1005"),
      retry: false,
      refetchInterval: 10000,
    });

  const { data: commercialBankAcc, isLoading: commercialBankLoading } =
    useQuery<Account>({
      queryKey: [`account-${2000}`],
      queryFn: () => getAccount("2000"),
      retry: false,
      refetchInterval: 10000,
    });

  const {
    data: transferPage,
    isLoading: transferPageLoading,
    error: transferPageError,
  } = useQuery<TransferPage>({
    queryKey: ["OverviewTransfers"],
    queryFn: async () => {
      var transfers = await getTransfers(undefined, 30);
      transfers.items = transfers.items.filter(transfer =>
        transfer.transferType === 'CompleteTransfer' ||
        transfer.transferType === 'Transfer'
      )
      return transfers;
    },
    refetchInterval: 5000,
  });

  return (
    <PageWrapper
      loading={
        ((retailLoading && retailAcc) ||
          (interestIncomeLoading && interestIncomeAcc) ||
          (feeIncomeLoading && feeIncomeAcc) ||
          (commercialBankLoading && commercialBankAcc) ||
          (transferPageLoading && transferPage)) ??
        false
      }
      error={transferPageError}
    >
      <main className="w-full h-full flex flex-row gap-2">
        <div className="scroll-auto min-h-[100%] w-[68%]">
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
              {feeIncomeAcc && (
                <AccountCard title="Fee Income" account={feeIncomeAcc} />
              )}
              {interestIncomeAcc && (
                <AccountCard
                  title="Interest Income"
                  account={interestIncomeAcc}
                />
              )}
            </div>
          </div>
          <Charts />
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
  const displayBalance = account.posted.balance + account.pending.balance
  const balanceCompare = displayBalance * (
    account.id == "1002" || account.id == "1005" ? -1 : 1
  );

  return (
    <Card
      onClick={() => account && navigate(`/accounts/${account.id}`)}
      className="cursor-pointer hover:shadow-lg hover:bg-gray-100 justify-between"
    >
      <CardHeader className="flex">
        <div className="flex gap-2">
          <BanknoteArrowUp className="w-6 h-6"/>
          <CardTitle className="text-xl font-light flex-1 leading-6">{title}</CardTitle>
        </div>
      </CardHeader>
      <CardContent>
        <div
          className={cn("flex justify-start text-lg font-mono", 
            balanceCompare > 0
            ? "text-green-600"
            : (balanceCompare < 0
            ? "text-red-600"
            : "text-gray-600")
          )}
        >
          {formatCurrency(displayBalance)}
        </div>
      </CardContent>
    </Card>
  );
}
