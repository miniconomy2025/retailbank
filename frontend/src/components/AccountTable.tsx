import { Link } from "react-router-dom";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import { Badge } from "./ui/badge";
import { accountLink, accountName, type Account } from "@/models/accounts";
import { formatMoney } from "@/utils/formatter";

export function AccountTable({ accounts, summary = false }: { accounts: Account[], summary?: boolean }) {
    return (
        <Table className="accounts-table">
            <TableHeader>
                <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Type</TableHead>
                    {
                        !summary &&
                        <>
                            <TableHead className="text-right">Debit Pending (Đ)</TableHead>
                            <TableHead className="text-right">Debit Posted (Đ)</TableHead>
                            <TableHead className="text-right">Credit Pending (Đ)</TableHead>
                            <TableHead className="text-right">Credit Posted (Đ)</TableHead>
                        </>
                    }
                    <TableHead className="text-right">Balance Pending (Đ)</TableHead>
                    <TableHead className="text-right">Balance Posted (Đ)</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                {accounts.map((account) => {
                    return (
                        <TableRow key={account.id}>
                            <TableCell className="text-left">
                                <Link to={accountLink(account.id)}>
                                    <span className="font-mono">{account.id}</span>
                                    <small className="ml-2 text-gray-500">{accountName(account.id)}</small>
                                </Link>
                            </TableCell>
                            <TableCell className="text-left">
                                <Badge variant="outline">{account.accountType}</Badge>
                            </TableCell>
                            {
                                !summary &&
                                <>
                                    <TableCell className="text-right font-mono">
                                        {formatMoney(account.pending.debits)}
                                    </TableCell>
                                    <TableCell className="text-right font-mono">
                                        {formatMoney(account.posted.debits)}
                                    </TableCell>
                                    <TableCell className="text-right font-mono">
                                        {formatMoney(account.pending.credits)}
                                    </TableCell>
                                    <TableCell className="text-right font-mono">
                                        {formatMoney(account.posted.credits)}
                                    </TableCell>
                                </>
                            }
                            <TableCell className="text-right font-mono">
                                {formatMoney(account.pending.balance)}
                            </TableCell>
                            <TableCell className="text-right font-mono">
                                {formatMoney(account.posted.balance)}
                            </TableCell>
                            <TableCell className="text-center">
                                <Badge
                                variant={account.closed ? "secondary" : "default"}
                                >
                                {account.closed ? "Closed" : "Active"}
                                </Badge>
                            </TableCell>
                        </TableRow>
                    );
                })}
            </TableBody>
        </Table>
    );
    
}
