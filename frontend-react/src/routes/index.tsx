import AuctionCard from "@/components/auction-card";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Auction } from "@/models/Auction";
import { allAuctionQueryOptions } from "@/utils/queryOptions";
import useAuctionsWebSocket from "@/utils/websocket/useAuctionsWebSocket";
import { useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/")({
  loader: (opts) =>
    opts.context.queryClient.ensureQueryData(allAuctionQueryOptions()),
  component: Index,
});

function Index() {
  const allAuctionsQuery = useSuspenseQuery(allAuctionQueryOptions());
  const allAuctions = allAuctionsQuery.data;

  const [isDataLoaded, setIsDataLoaded] = useState(false);

  useEffect(() => {
    if (allAuctions) {
      setIsDataLoaded(true);
    }
  }, [allAuctions]);

  useAuctionsWebSocket(isDataLoaded);

  return (
    <>
      <ScrollArea className="max-h-[590px] min-w-max rounded-md border p-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {allAuctions.map((auction: Auction) => (
            <AuctionCard key={auction.Auction_Id} {...auction} />
          ))}
        </div>
      </ScrollArea>
    </>
  );
}