import { useState } from "react";
import { Search, Eye } from "lucide-react";
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
import { useQuery } from "@tanstack/react-query";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useNavigate } from "react-router-dom";
import { TransferEventType, type Transfer } from "@/models/transfers";
import { getTransfers } from "@/api/transfers";

export default function Transfers() {
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  const {
    data: transfers,
    isLoading,
    error,
  } = useQuery<Transfer[]>({
    queryKey: ["transfers"],
    queryFn: () => getTransfers(),
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

  const totalTransfers = transfers?.length;
  const totalAmount = transfers
    ?.filter((t) => t.eventType === TransferEventType.TRANSFER)
    .reduce((sum, t) => sum + t.amount, 0);

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="flex flex-col gap-4">
        <div className="flex ">
          <div>
            <h1 className="text-3xl font-bold text-left">Transfers</h1>
            <p className="text-left">All transfers in the system</p>
          </div>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total transfers</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">{totalTransfers}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Amount</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(totalAmount)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Amount</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(totalAmount)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Amount</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {formatCurrency(totalAmount)}
              </div>
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
                    <TableHead>From Account</TableHead>
                    <TableHead>To Account</TableHead>
                    <TableHead className="text-right">Amount</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="text-center"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredTransfers?.map((transfer) => (
                    <TableRow key={transfer.transactionId}>
                      <TableCell className="text-left">
                        {transfer.transactionId}
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
                      <TableCell className="text-center">
                        <Eye
                          className="h-6 w-6 cursor-pointer"
                          onClick={() =>
                            navigate(`/transfers/${transfer.transactionId}`)
                          }
                        />
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
