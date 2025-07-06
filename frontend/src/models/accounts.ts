export interface Account {
  id: string;
  accountType: LedgerAccountType;
  debitsPending: number;
  debitsPosted: number;
  creditsPending: number;
  creditsPosted: number;
  balancePending: number;
  balancePosted: number;
  closed: boolean;
  createdAt: number;
}

export interface AccountPage {
  items: Account[];
  next?: string;
}

export type LedgerAccountType = "Internal" | "Transactional" | "Loan";
