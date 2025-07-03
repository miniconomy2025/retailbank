import { useEffect, useRef, useState } from "react";
import { Badge } from "@/components/ui/badge";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useInfiniteQuery } from "@tanstack/react-query";
import PageWrapper from "@/components/PageWrapper";
import { formatCurrency } from "@/utils/formatter";
import { type TransferPage } from "@/models/transfers";
import { getTransfers } from "@/api/transfers";
import { Eye, Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";

export default function Transfers() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");

  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery<TransferPage>({
      queryKey: ["transfers"],
      queryFn: ({ pageParam }) => getTransfers(pageParam as string | undefined),
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

  const transfers = data?.pages.flatMap((page) => page.items) ?? [];

  const handleSearch = () => {
    if (searchTerm.trim()) {
      navigate(`/transfers/${searchTerm.trim()}`);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="h-full flex flex-col gap-4">
        <h1 className="text-3xl font-bold text-left">Transfers</h1>

        <div className="flex gap-2">
          <div className="relative flex-1">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4" />
            <Input
              placeholder="Transfer ID"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value.slice(0, 31))}
              onKeyDown={handleKeyPress}
              className="pl-8"
            />
          </div>
          <Button
            onClick={handleSearch}
            disabled={!searchTerm.trim()}
            className="px-6"
          >
            Go to Transfer
          </Button>
        </div>
        <div className="rounded-md border overflow-auto">
          {transfers?.length === 0 ? (
            <div className="text-center py-8 ">No transfers found</div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>From</TableHead>
                    <TableHead>To</TableHead>
                    <TableHead className="text-right">Amount</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="text-center"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {transfers?.map((transfer) => (
                    <TableRow key={transfer.transactionId + transfer.timestamp}>
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
                            navigate(`/transfers/${transfer?.transactionId}`)
                          }
                        />
                      </TableCell>
                    </TableRow>
                  ))}
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
