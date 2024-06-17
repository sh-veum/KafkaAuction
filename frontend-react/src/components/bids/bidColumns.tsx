import { Bid } from "@/models/Bid";
import { ColumnDef } from "@tanstack/react-table";
import { format, isToday, parseISO } from "date-fns";

export const bidColumns: ColumnDef<Bid>[] = [
  {
    accessorKey: "Username",
    header: "Username",
  },
  {
    accessorKey: "Bid_Amount",
    header: () => <div className="text-right">Amount</div>,
    cell: ({ row }) => {
      const amount = parseFloat(row.getValue("Bid_Amount"));
      const formatted = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
      }).format(amount);

      return <div className="text-right font-medium">{formatted}</div>;
    },
  },
  {
    accessorKey: "Timestamp",
    header: "Timestamp",
    cell: ({ row }) => {
      const timestamp = row.getValue("Timestamp") as string;
      const date = parseISO(timestamp);

      let formattedDate;
      if (isToday(date)) {
        formattedDate = format(date, "HH:mm:ss");
      } else {
        formattedDate = format(date, "MMMM d, yyyy 'at' HH:mm");
      }

      return <div>{formattedDate}</div>;
    },
    sortingFn: "datetime",
  },
];
