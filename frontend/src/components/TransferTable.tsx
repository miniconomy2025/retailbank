import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TransferTypeInfo, type Transfer } from "@/models/transfers";
import { formatMoney } from "@/utils/formatter";
import { Badge } from "./ui/badge";
import { Link } from "react-router-dom";
import { accountLink, accountName } from "@/models/accounts";

export default function TransferTable({ transfers, accountId }: { transfers: Transfer[], accountId?: string }) {
  return (
        <Table className="transfer-table">
            <TableHeader>
                <TableRow>
                    <TableHead>Transfer ID</TableHead>
                    <TableHead>Parent Transfer ID</TableHead>
                    <TableHead>Reference</TableHead>
                    {
                        accountId
                        ? <>
                            <TableHead>Account</TableHead>
                            <TableHead>Debit (Đ)</TableHead>
                            <TableHead>Credit (Đ)</TableHead>
                        </>
                        : <>
                            <TableHead>Debit Account Number</TableHead>
                            <TableHead>Credit Account Number</TableHead>
                            <TableHead className="text-right">Amount (Đ)</TableHead>
                        </>
                    }
                    <TableHead className="text-center">Transfer Event Type</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                {transfers?.map((transfer) => (
                    <TableRow id={`transfer-${transfer.transferId}`} className={TransferTypeInfo[transfer.transferType].className} key={transfer.transferId}>
                        <TableCell className="text-left text-xs font-mono">
                            {transfer.transferId}
                        </TableCell>
                        <TableCell className="text-left">
                            {
                                transfer.parentId
                                ? <a className="font-mono text-xs" href={`#transfer-${transfer.parentId}`}>{transfer.parentId}</a>
                                : <></>
                            }
                        </TableCell>
                        <TableCell className="text-left font-mono">
                            {transfer.reference > 0 && transfer.reference}
                        </TableCell>
                        {
                            accountId
                            ? <>
                                {
                                    transfer.debitAccountId === accountId
                                    ? <>
                                        <TableCell className="text-left">
                                            <Link to={accountLink(transfer.creditAccountId, transfer.transferId)}>
                                                <span className="font-mono">{transfer.creditAccountId}</span>
                                                <small className="ml-2 text-gray-500">{accountName(transfer.creditAccountId)}</small>
                                            </Link>
                                        </TableCell>
                                        <TableCell className="text-left transfer-amount font-mono">
                                            <span>{formatMoney(transfer.amount)}</span>
                                        </TableCell>
                                        <TableCell/>
                                    </>
                                    : <>
                                        <TableCell className="text-left">
                                            <Link to={accountLink(transfer.debitAccountId, transfer.transferId)}>
                                                <span className="font-mono">{transfer.debitAccountId}</span>
                                                <small className="ml-2 text-gray-500">{accountName(transfer.debitAccountId)}</small>
                                            </Link>
                                        </TableCell>
                                        <TableCell/>
                                        <TableCell className="text-left transfer-amount font-mono">
                                            <span>{formatMoney(transfer.amount)}</span>
                                        </TableCell>
                                    </>
                                }
                            </>
                            : <>
                                <TableCell className="text-left">
                                    <Link to={accountLink(transfer.debitAccountId, transfer.transferId)}>
                                        <span className="font-mono">{transfer.debitAccountId}</span>
                                        <small className="ml-2 text-gray-500">{accountName(transfer.debitAccountId)}</small>
                                    </Link>
                                </TableCell>
                                <TableCell className="text-left">
                                    <Link to={accountLink(transfer.creditAccountId, transfer.transferId)}>
                                        <span className="font-mono">{transfer.creditAccountId}</span>
                                        <small className="ml-2 text-gray-500">{accountName(transfer.creditAccountId)}</small>
                                    </Link>
                                </TableCell>
                                <TableCell className="text-right transfer-amount font-mono">
                                    {formatMoney(transfer.amount)}
                                </TableCell>
                            </>
                        }
                        <TableCell className="text-center">
                            <Badge className="transfer-event-type">{TransferTypeInfo[transfer.transferType].displayName}</Badge>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
}
