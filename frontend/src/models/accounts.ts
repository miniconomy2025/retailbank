export interface Account {
  id: string;
  accountType: LedgerAccountType;
  pending: Balance;
  posted: Balance;
  closed: boolean;
  createdAt: number;
}

export interface Balance {
  debits: number;
  credits: number;
  balance: number;
}

export interface AccountPage {
  items: Account[];
  next?: string;
}

export type LedgerAccountType = "Internal" | "Transactional" | "Loan";

export const AccountNames = {
  "1002": "Interest Income",
  "1004": "Bad Debts",
  "1005": "Fee Income",
  "1000": "Retail Bank",
  "2000": "Commercial Bank",
};

export function accountName(accountId: string): string | null {
  if (accountId in AccountNames)
    return AccountNames[accountId as keyof typeof AccountNames];
  return null;
}

export function accountLocalId(accountId: string): string {
  let bankCode = accountId.substring(0, 4);
  if (bankCode === "1000") {
    return accountId;
  } else {
    return bankCode;
  }
}

export function accountLink(accountId: string, transferId: string | undefined = undefined): string {
  return `/accounts/${accountLocalId(accountId)}${transferId ? `#transfer-${transferId}` : ''}`
}
