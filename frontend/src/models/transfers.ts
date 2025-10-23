export interface Transfer {
  transferId: string;
  debitAccountId: string;
  creditAccountId: string;
  amount: number;
  parentId?: string;
  transferType: TransferType;
  reference: number;
  timestamp: string;
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

export const TransferTypeInfo = {
  "Transfer": {
    displayName: "Transfer",
    className: "transfer-transfer",
  },
  "StartTransfer": {
    displayName: "Start Transfer",
    className: "transfer-start-transfer",
  },
  "CompleteTransfer": {
    displayName: "Complete Transfer",
    className: "transfer-complete-transfer",
  },
  "CancelTransfer": {
    displayName: "Cancel Transfer",
    className: "transfer-cancel-transfer",
  },
  "BalanceDebit": {
    displayName: "Balance Debit",
    className: "transfer-balance-debit",
  },
  "BalanceCredit": {
    displayName: "Balance Credit",
    className: "transfer-balance-credit",
  },
  "CloseDebit": {
    displayName: "Close Debit",
    className: "transfer-close-debit",
  },
  "CloseCredit": {
    displayName: "Close Credit",
    className: "transfer-close-credit",
  },
};
