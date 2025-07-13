import { useEffect, useRef } from "react";

import { useInfiniteQuery } from "@tanstack/react-query";
import PageWrapper from "@/components/PageWrapper";
import { type TransferPage } from "@/models/transfers";
import { getTransfers } from "@/api/transfers";
import TransferTable from "@/components/TransferTable";

export default function Transfers() {
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

  return (
    <PageWrapper loading={isLoading} error={error}>
      <div className="h-full flex flex-col gap-4">
        <h1 className="text-3xl font-bold text-left">Transfers</h1>

        <div className="rounded-md border overflow-auto">
          {transfers?.length === 0 ? (
            <div className="text-center py-8 ">No transfers found</div>
          ) : (
            <>
              <TransferTable transfers={transfers}/>
              <div ref={bottomRef} className="h-px" />
            </>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}
