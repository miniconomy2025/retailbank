import { Search } from "lucide-react";

import { type AccountPage } from "@/models/accounts";
import { useInfiniteQuery } from "@tanstack/react-query";
import { getAccounts } from "@/api/accounts";
import PageWrapper from "@/components/PageWrapper";
import { useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { AccountTable } from "@/components/AccountTable";

export default function Accounts() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");

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

  const handleSearch = () => {
    if (searchTerm.trim()) {
      navigate(`/accounts/${searchTerm.trim()}`);
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
        <h1 className="text-3xl font-bold text-left">Accounts</h1>

        <div className="flex gap-2">
          <div className="relative flex-1">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4" />
            <Input
              placeholder="Account ID"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value.slice(0, 13))}
              onKeyDown={handleKeyPress}
              className="pl-8"
              type="number"
            />
          </div>
          <Button
            onClick={handleSearch}
            disabled={!searchTerm.trim()}
            className="px-6"
          >
            Go to Account
          </Button>
        </div>
        <div className="rounded-md border overflow-auto">
          {accounts?.length === 0 ? (
            <div className="text-center py-8 ">No accounts found</div>
          ) : (
            <>
              <AccountTable accounts={accounts}/>
              <div ref={bottomRef} className="h-px" />
            </>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}
