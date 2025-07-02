import { useState } from "react";
import { Search, Filter, Eye } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { type Account } from "@/models/accounts";
import { useQuery } from "@tanstack/react-query";
import { getAccounts } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useNavigate } from "react-router-dom";

export default function Accounts() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const navigate = useNavigate();

  const {
    data: accounts,
    isLoading,
    error,
  } = useQuery<Account[]>({
    queryKey: ["accounts"],
    queryFn: getAccounts,
    refetchInterval: 5000,
  });

  const filteredAccounts = accounts?.filter((account) => {
    const matchesSearch =
      account.id.toString().includes(searchTerm.toLowerCase()) ||
      account.accountType
        .toString()
        .toLowerCase()
        .includes(searchTerm.toLowerCase());

    const matchesStatus =
      statusFilter === "all" ||
      (statusFilter === "active" && !account.closed) ||
      (statusFilter === "closed" && account.closed);

    return matchesSearch && matchesStatus;
  });

  const totalAccounts = accounts?.length;
  const activeAccounts = accounts?.filter((acc) => !acc.closed).length;
  const closedAccounts = accounts?.filter((acc) => acc.closed).length;
  const totalDebitsPosted = accounts?.reduce(
    (sum, acc) => sum + acc.debitsPosted,
    0
  );
  const totalCreditsPosted = accounts?.reduce(
    (sum, acc) => sum + acc.creditsPosted,
    0
  );

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="flex flex-col gap-4">
        <div className="flex ">
          <div>
            <h1 className="text-3xl font-bold text-left">Accounts</h1>
            <p>Monitor your ledger accounts</p>
          </div>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Accounts</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{totalAccounts}</div>
              <p className="text-s">
                {activeAccounts} active, {closedAccounts} closed
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Total Debits Posted</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {formatCurrency(totalDebitsPosted)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">
                Total Credits Posted
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {formatCurrency(totalCreditsPosted)}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex">
              <CardTitle className="font-medium">Net Balance</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {formatCurrency(
                  (totalDebitsPosted ?? 0) - (totalCreditsPosted ?? 0)
                )}
              </div>
            </CardContent>
          </Card>
        </div>
        <Card>
          <CardContent className="flex flex-col gap-4">
            <div className="flex items-center gap-2">
              <div className="relative flex-1">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4" />
                <Input
                  placeholder="Search accounts by ID or type..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-40 cursor-pointer">
                  <Filter />
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem className="cursor-pointer" value="all">
                    All Accounts
                  </SelectItem>
                  <SelectItem className="cursor-pointer" value="active">
                    Active Only
                  </SelectItem>
                  <SelectItem className="cursor-pointer" value="closed">
                    Closed Only
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Accounts Table */}
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Account ID</TableHead>
                    <TableHead>Account Type</TableHead>
                    <TableHead className="text-right">Debits Pending</TableHead>
                    <TableHead className="text-right">Debits Posted</TableHead>
                    <TableHead className="text-right">
                      Credits Pending
                    </TableHead>
                    <TableHead className="text-right">Credits Posted</TableHead>
                    <TableHead className="text-right">Balance</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="text-center"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredAccounts?.map((account) => {
                    const balance =
                      account.debitsPosted - account.creditsPosted;
                    return (
                      <TableRow key={account.id}>
                        <TableCell className="text-left">
                          {account.id}
                        </TableCell>
                        <TableCell className="text-left">
                          <Badge variant="outline">{account.accountType}</Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(account.debitsPending)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(account.debitsPosted)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(account.creditsPending)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(account.creditsPosted)}
                        </TableCell>
                        <TableCell
                          className={`text-right ${
                            balance > 0
                              ? "text-green-600"
                              : balance < 0
                              ? "text-red-600"
                              : "text-gray-600"
                          }`}
                        >
                          {formatCurrency(balance)}
                        </TableCell>
                        <TableCell className="text-center">
                          <Badge
                            variant={account.closed ? "secondary" : "default"}
                          >
                            {account.closed ? "Closed" : "Active"}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-center">
                          <Eye
                            className="h-6 w-6 cursor-pointer"
                            onClick={() => navigate(`/accounts/${account.id}`)}
                          />
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>

            {filteredAccounts?.length === 0 && (
              <div className="text-center py-8 ">
                No accounts found matching your criteria.
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </PageWrapper>
  );
}
