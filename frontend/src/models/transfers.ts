export interface Transfer {
  transactionId: string;
  debitAccountNumber: number;
  creditAccountNumber: number;
  amount: number;
  pendingId: string | null;
  timestamp: bigint;
  eventType: TransferEventType;
}

enum TransferEventType {
  Transfer = "Transfer",
  StartTransfer = "StartTransfer",
  CompleteTransfer = "CompleteTransfer",
  CancelTransfer = "CancelTransfer",
  ClosingCredit = "ClosingCredit",
  ClosingDebit = "ClosingDebit",
  ReopenCredit = "ReopenCredit",
  ReopenDebit = "ReopenDebit",
}