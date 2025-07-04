export interface Account {
  id: number;
  accountType: LedgerAccountType;
  debitsPending: number;
  debitsPosted: number;
  creditsPending: number;
  creditsPosted: number;
  closed: boolean;
  createdAt?: number;
  balancePending: number;
  balancePosted: number;
}

export interface AccountPage {
  items: Account[];
  next?: string | null;
}

enum LedgerAccountType {
  BANK = "Bank",
  INTERNAL = "Internal",
  TRANSACTIONAL = "Transactional",
  LOAN = "Loan",
}
