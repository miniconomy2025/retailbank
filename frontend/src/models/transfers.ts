export interface Transfer {
  transferId: string;
  debitAccountId: string;
  creditAccountId: string;
  amount: number;
  parentId?: string;
  timestamp: number;
  transferType: TransferType;
  reference: number;
}

export type TransferType =
  "Transfer" |
  "StartTransfer" |
  "CompleteTransfer" |
  "CancelTransfer" |
  "BalanceDebit" |
  "BalanceCredit" |
  "CloseDebit" |
  "CloseCredit";

export interface TransferPage {
  items: Transfer[];
  next?: string | null;
}