import { useQuery } from "@tanstack/react-query";
import LiveChart, { type DataPoint } from "./LiveChart";
import type { ReportData } from "@/models/overview";
import { useEffect, useState } from "react";
import { getReport } from "@/api/overview";

const skyBlue = "rgba(135, 206, 250, 0.7)";
const mintGreen = "rgba(144, 238, 144, 0.7)";
const peach = "rgba(255, 218, 185, 0.7)";
const pastelPurple = "rgba(216, 191, 216, 0.7)";

export default function Charts() {
  const [bankBalanceDataPoints, setBankBalanceDataPoints] = useState<
    DataPoint[]
  >([]);
  const [transactionAccountsDataPoints, setTransactionalAccountsDataPoints] =
    useState<DataPoint[]>([]);
  const [loanAccountsDataPoints, setLoanAccountDataPoints] = useState<
    DataPoint[]
  >([]);
  const [totalEconomicVolume, setTotalEconomic] = useState<DataPoint[]>([]);

  const { data: reportData } = useQuery<ReportData>({
    queryKey: ["report"],
    queryFn: getReport,
    refetchInterval: 10000,
  });

  useEffect(() => {
    if (!reportData) return;

    setBankBalanceDataPoints((prev) => [
      ...prev.slice(-30),
      { time: reportData.timestamp, value: reportData.bankBalance },
    ]);
    setLoanAccountDataPoints((prev) => [
      ...prev.slice(-30),
      { time: reportData.timestamp, value: reportData.loanAccounts },
    ]);
    setTotalEconomic((prev) => [
      ...prev.slice(-30),
      { time: reportData.timestamp, value: reportData.totalMoney },
    ]);
    setTransactionalAccountsDataPoints((prev) => [
      ...prev.slice(-30),
      { time: reportData.timestamp, value: reportData.transactionalAccounts },
    ]);
  }, [reportData]);
  return (
    <div className="grid md:grid-cols-1 lg:grid-cols-2 gap-2 mt-2 scroll-auto">
      <div className="h-60">
        <LiveChart
          dataPoints={bankBalanceDataPoints}
          title="Bank Balance"
          label="Total Balance"
          chartColor={skyBlue}
        />
      </div>
      <div className="h-60">
        <LiveChart
          dataPoints={loanAccountsDataPoints}
          title="Loan Accounts"
          label="No. of accounts"
          chartColor={mintGreen}
        />
      </div>
      <div className="h-60">
        <LiveChart
          dataPoints={totalEconomicVolume}
          title="Economic Volume"
          label="Total Volume"
          chartColor={pastelPurple}
        />
      </div>
      <div className="h-60">
        <LiveChart
          dataPoints={transactionAccountsDataPoints}
          title="Transaction Accounts"
          label="No. of accounts"
          chartColor={peach}
        />
      </div>
    </div>
  );
}
