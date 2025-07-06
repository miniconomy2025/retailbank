import PageWrapper from "@/components/PageWrapper";
import { useParams } from "react-router-dom";
import { getTransfer } from "@/api/transfers";
import { useQuery } from "@tanstack/react-query";
import type { Transfer } from "@/models/transfers";
import { formatCurrency } from "@/utils/formatter";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";

export default function TransferPage() {
  const { transferId } = useParams();

  const {
    data: transfer,
    isLoading,
    error,
  } = useQuery<Transfer>({
    queryKey: [`transfer-${transferId}`],
    queryFn: () => getTransfer(transferId ?? ""),
    retry: false,
  });

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="w-full h-full flex items-center justify-center">
        <Card className="max-w-md w-full">
          <CardHeader className="text-center">
            <CardTitle className="text-2xl">Transfer</CardTitle>
          </CardHeader>

          <CardContent>
            <div className="text-center">
              <div className="text-3xl font-bold mb-2">
                {formatCurrency(transfer?.amount ?? 0)}
              </div>
            </div>
            <div>
              <div className="flex items-center justify-center w-full py-2">
                <div>
                  <p className="font-medium">From Account</p>
                  <p>{transfer?.debitAccountId}</p>
                </div>
              </div>
              <div className="flex items-center justify-center w-full py-2">
                <div>
                  <p className="font-medium">To Account</p>
                  <p>{transfer?.creditAccountId}</p>
                </div>
              </div>
            </div>
            <div>
              <h3 className="text-2xl py-4">Transaction Details</h3>
              <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span>Pending ID</span>
                  <div>
                    {transfer?.parentId ? (
                      <span>{transfer?.parentId}</span>
                    ) : (
                      <span>N/A</span>
                    )}
                  </div>
                </div>

                <div className="flex items-center justify-between">
                  <span>Status</span>
                  <div>{transfer?.transferType}</div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </PageWrapper>
  );
}
