import { Eye } from "lucide-react";
import { Badge } from "@/components/ui/badge";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { type AccountPage } from "@/models/accounts";
import { useInfiniteQuery } from "@tanstack/react-query";
import { getAccounts } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { useNavigate } from "react-router-dom";
import { useEffect, useRef } from "react";

export default function Accounts() {
  const navigate = useNavigate();

  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery<AccountPage>({
      queryKey: ["accounts"],
      queryFn: ({ pageParam }) => getAccounts(pageParam as string | undefined),
      getNextPageParam: (lastPage) => lastPage.next || undefined,
      initialPageParam: undefined,
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

  const accounts = data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="h-full flex flex-col gap-4">
        <h1 className="text-3xl font-bold text-left">Accounts</h1>
        <div className="rounded-md border overflow-auto">
          {accounts?.length === 0 ? (
            <div className="text-center py-8 ">No accounts found</div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead className="text-right">Debit Pending</TableHead>
                    <TableHead className="text-right">Debit Posted</TableHead>
                    <TableHead className="text-right">Credit Pending</TableHead>
                    <TableHead className="text-right">Credit Posted</TableHead>
                    <TableHead className="text-right">
                      Balance Pending
                    </TableHead>
                    <TableHead className="text-right">Balance Posted</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="text-center"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {accounts?.map((account) => {
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
                        <TableCell className="text-right">
                          {formatCurrency(account.balancePending)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(account.balancePosted)}
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
              <div ref={bottomRef} className="h-px" />
            </>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}
