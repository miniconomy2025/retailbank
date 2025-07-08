import { getAccount } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Account } from "@/models/accounts";
import { formatCurrency } from "@/utils/formatter";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { Activity, BanknoteArrowUp} from 'lucide-react';
import {  type TransferPage } from "@/models/transfers";
import { getTransfers } from "@/api/transfers";
import { useEffect, useState } from "react";
import type { DataPoint } from "@/components/LiveChart";
import LiveChart from "@/components/LiveChart";

const skyBlue = 'rgba(135, 206, 250, 0.7)';
const mintGreen = 'rgba(144, 238, 144, 0.7)';
const peach = 'rgba(255, 218, 185, 0.7)';
const pastelPurple = 'rgba(216, 191, 216, 0.7)';

export default function Overview() {
  const {
    data: retailAcc,
    isLoading: retailLoading,
    error: retailErr,
  } = useQuery<Account>({
    queryKey: [`account-${1000}`],
    queryFn: () => getAccount("1000"),
    retry: false,
    refetchInterval: 5000
  });

  const {
    data: interestIncomeAcc,
    isLoading: interestIncomeLoading,
    error: interestIncomeErr,
  } = useQuery<Account>({
    queryKey: [`account-${1002}`],
    queryFn: () => getAccount("1002"),
    retry: false,
    refetchInterval: 5000
  });

  const {
    data: loanControlAcc,
    isLoading: loanControlLoading,
    error: loanControlErr,
  } = useQuery<Account>({
    queryKey: [`account-${1003}`],
    queryFn: () => getAccount("1003"),
    retry: false,
    refetchInterval: 5000
  });

  const {
    data: commercialBankAcc,
    isLoading: commercialBankLoading,
    error: commercialBankErr,
  } = useQuery<Account>({
    queryKey: [`account-${2000}`],
    queryFn: () => getAccount("2000"),
    retry: false,
    refetchInterval:5000
  });

  const {
    data: transferPage,
    isLoading: transferPageLoading,
    error: transferPageError
  } = useQuery<TransferPage>({
    queryKey: ['transfers'],
    queryFn: () => getTransfers(undefined, 30),
    refetchInterval: 5000,
  });

  const [bankBalanceDataPoints, setBankBalanceDataPoints] = useState<DataPoint[]>([]);
  const [transactionAccountsDataPoints, setTransactionalAccountsDataPoints] = useState<DataPoint[]>([]);
  const [loanAccountsDataPoints, setLoanAccountDataPoints] = useState<DataPoint[]>([]);
  const [totalEconomicVolume, setTotalEconomic] = useState<DataPoint[]>([]);


  useEffect(() => {
    const interval = setInterval(() => {
      fetch('/api/report')
        .then((res) => res.json())
        .then((newData) => {
          const bankBalancePoint = {
            time: new Date().toLocaleTimeString(),
            value: newData.bankBalance ,
          };
          const loanAccountPoint = {
            time: new Date().toLocaleTimeString(),
            value: newData.loanAccounts,
          };
          const totalVolumePoint = {
            time: new Date().toLocaleTimeString(),
            value: newData.totalMoney,
          };
          const transactionalAccountPoint = {
            time: new Date().toLocaleTimeString(),
            value: newData.transactionalAccounts,
          };

          setBankBalanceDataPoints((prev) => [...prev.slice(-59), bankBalancePoint]);
          setLoanAccountDataPoints((prev)=>  [...prev.slice(-59), loanAccountPoint]);
          setTotalEconomic((prev)=>  [...prev.slice(-59), totalVolumePoint]);
          setTransactionalAccountsDataPoints((prev)=>  [...prev.slice(-59), transactionalAccountPoint]);
        })
    }, 5000);

    return () => clearInterval(interval);
  }, []);

  return (
    <PageWrapper
      loading={
        retailLoading ||
        interestIncomeLoading ||
        loanControlLoading ||
        commercialBankLoading ||
        transferPageLoading
      }
      error={
        retailErr ||
        interestIncomeErr ||
        loanControlErr ||
        commercialBankErr ||
        transferPageError
      }
    >
      <main className="w-full h-full flex flex-row gap-2">
        <div className="scroll-auto min-h-[100%] w-[70%]">
          <div className=" flex flex-col gap-4">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              {retailAcc && <AccountCard title="Retail Bank" account={retailAcc} />}
              {commercialBankAcc && <AccountCard title="Commercial Bank" account={commercialBankAcc} />}
              {loanControlAcc && <AccountCard title="Loan Control" account={loanControlAcc} />}
              {interestIncomeAcc && <AccountCard title="Interest Income" account={interestIncomeAcc} />}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-2 mt-2 scroll-auto">
              <div className="h-64"><LiveChart dataPoints={bankBalanceDataPoints} title="Bank Balance" label="Total Balance" chartColor={skyBlue}/></div>
              <div className="h-64"><LiveChart dataPoints={loanAccountsDataPoints} title="Loan Accounts" label="No. of accounts" chartColor={mintGreen} /></div>
              <div className="h-64"><LiveChart dataPoints={transactionAccountsDataPoints} title="Transaction Accounts" label="No. of accounts" chartColor={peach}  /></div>
              <div className="h-64"><LiveChart dataPoints={totalEconomicVolume} title="Economic Volume" label="Total Volume" chartColor={pastelPurple} /></div>
          </div>
        </div>
        <div className="flex-col h-full flex-1 p-4 rounded-2xl shadow-lg border-slate-100 border-2">
          <div className="flex flex-row items-center gap-2 mb-4">
            <Activity />
            <h3 className="text-lg font-semibold">Recent Transfers</h3>
          </div>

          <div className="overflow-x-auto h-[90%] overflow-y-auto border shadow-sm">
            <table className="min-w-full table-auto text-left text-sm">
              <thead className=" bg-slate-100 text-slate-800">
                <tr>
                  <th className="px-4 py-2">DR Acc</th>
                  <th className="px-4 py-2">CR Acc</th>
                  <th className="px-4 py-2">Amount</th>
                </tr>
              </thead>
              <tbody>
                {transferPage?.items.map((item, index) => (
                  <tr
                    key={index}
                    className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
                  >
                    <td className="px-4 py-2">{item.debitAccountId}</td>
                    <td className="px-4 py-2">{item.creditAccountId}</td>
                    <td
                      className={`px-4 py-2 font-semibold ${item.debitAccountId != '1000' ? (item.creditAccountId === '1000' ? "text-red-600" : "text-yellow-400") : "text-green-600"
                        }`}
                    >
                      {formatCurrency(item.amount)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </PageWrapper>
  );
}

function AccountCard({ title, account }: { title: string; account: Account }) {
  const navigate = useNavigate();
  const displayBalance = account.id == '1002' ? account.balancePosted * -1 : account.balancePosted
  
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
        <div className={displayBalance <= 0 ? "flex justify-start font-extrabold text-lg text-bol text-red-500" : "flex justify-start font-extrabold text-lg text-bol text-green-500"}>
          {formatCurrency(displayBalance)}
        </div>
      </CardContent>
    </Card>
  );
}
