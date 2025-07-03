export interface Transfer {
  transactionId: string;
  debitAccountNumber: number;
  creditAccountNumber: number;
  amount: number;
  pendingId: string | null;
  timestamp: bigint;
  eventType: TransferEventType;
}

export enum TransferEventType {
  TRANSFER = "Transfer",
  START_TRANSFER = "StartTransfer",
  COMPLETE_TRANSFER = "CompleteTransfer",
  CANCEL_TRANSFER = "CancelTransfer",
  CLOSING_CREDIT = "ClosingCredit",
  CLOSING_DEBIT = "ClosingDebit",
  REOPEN_CREDIT = "ReopenCredit",
  REOPEN_DEBIT = "ReopenDebit",
}

export interface TransferPage {
  items: Transfer[];
  next?: string | null;
}