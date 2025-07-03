import { getAccount } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Account } from "@/models/accounts";
import { formatCurrency } from "@/utils/formatter";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

export default function Overview() {
  const {
    data: retailAcc,
    isLoading: retailLoading,
    error: retailErr,
  } = useQuery<Account>({
    queryKey: [`account-${1000}`],
    queryFn: () => getAccount(1000),
  });

  const {
    data: ownersEquityAcc,
    isLoading: ownersEquityLoading,
    error: ownersEquityErr,
  } = useQuery<Account>({
    queryKey: [`account-${1001}`],
    queryFn: () => getAccount(1001),
  });

  const {
    data: interestIncomeAcc,
    isLoading: interestIncomeLoading,
    error: interestIncomeErr,
  } = useQuery<Account>({
    queryKey: [`account-${1002}`],
    queryFn: () => getAccount(1002),
  });

  const {
    data: loanControlAcc,
    isLoading: loanControlLoading,
    error: loanControlErr,
  } = useQuery<Account>({
    queryKey: [`account-${1003}`],
    queryFn: () => getAccount(1003),
  });

  const {
    data: badDebtsAcc,
    isLoading: badDebtsLoading,
    error: badDebtsErr,
  } = useQuery<Account>({
    queryKey: [`account-${1004}`],
    queryFn: () => getAccount(1004),
  });

  const {
    data: commercialBankAcc,
    isLoading: commercialBankLoading,
    error: commercialBankErr,
  } = useQuery<Account>({
    queryKey: [`account-${2000}`],
    queryFn: () => getAccount(2000),
  });

  return (
    <PageWrapper
      loading={
        retailLoading ||
        ownersEquityLoading ||
        interestIncomeLoading ||
        loanControlLoading ||
        badDebtsLoading ||
        commercialBankLoading
      }
      error={
        retailErr ||
        ownersEquityErr ||
        interestIncomeErr ||
        loanControlErr ||
        badDebtsErr ||
        commercialBankErr
      }
    >
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <AccountCard title="Retail Bank" account={retailAcc} />
        <AccountCard title="Owner’s Equity" account={ownersEquityAcc} />
        <AccountCard title="Interest Income" account={interestIncomeAcc} />
        <AccountCard title="Loan Control" account={loanControlAcc} />
        <AccountCard title="Bad Debts" account={badDebtsAcc} />
        <AccountCard title="Commercial Bank" account={commercialBankAcc} />
      </div>
    </PageWrapper>
  );
}

function AccountCard({ title, account }: { title: string; account?: Account }) {
  const navigate = useNavigate();

  return (
    <Card
      onClick={() => account && navigate(`/accounts/${account.id}`)}
      className="cursor-pointer hover:shadow-lg hover:bg-gray-100"
    >
      <CardHeader className="flex">
        <CardTitle className="text-xl font-bold">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-xl font-bold">
          Balance: {formatCurrency(account?.balancePosted)}
        </div>
        <p className="text-s">
          Pending: {formatCurrency(account?.balancePending)}
        </p>
      </CardContent>
    </Card>
  );
}
