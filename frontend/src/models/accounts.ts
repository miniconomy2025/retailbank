export interface Account {
  id: number;
  accountType: LedgerAccountCode;
  debitsPending: number;
  debitsPosted: number;
  creditsPending: number;
  creditsPosted: number;
  closed: boolean;
}

enum LedgerAccountCode {
  Bank = 1000,
  Internal = 2000,
  Transactional = 3000,
  Loan = 4000,
}

export const sampleAccounts: Account[] = [
  {
    id: 1000,
    accountType: LedgerAccountCode.Bank,
    debitsPending: 15000.0,
    debitsPosted: 125000.5,
    creditsPending: 2500.0,
    creditsPosted: 98750.25,
    closed: false,
  },
  {
    id: 1001,
    accountType: LedgerAccountCode.Internal,
    debitsPending: 15000.0,
    debitsPosted: 125000.5,
    creditsPending: 2500.0,
    creditsPosted: 98750.25,
    closed: false,
  },
  {
    id: 1002,
    accountType: LedgerAccountCode.Transactional,
    debitsPending: 15000.0,
    debitsPosted: 125000.5,
    creditsPending: 2500.0,
    creditsPosted: 98750.25,
    closed: false,
  },
  {
    id: 1003,
    accountType: LedgerAccountCode.Loan,
    debitsPending: 15000.0,
    debitsPosted: 125000.5,
    creditsPending: 2500.0,
    creditsPosted: 98750.25,
    closed: false,
  },
];
