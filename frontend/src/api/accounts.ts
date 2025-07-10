import { type Account, type AccountPage } from "@/models/accounts";
import type { TransferPage } from "@/models/transfers";
import { apiFetch } from "@/utils/api";

export async function getAccounts(
  nextUrl?: string
): Promise<AccountPage> {
  const response = await apiFetch({
    path: nextUrl || `/accounts?limit=25`,
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  return await response.json();
}

export async function getAccount(accountId: string): Promise<Account> {
  const response = await apiFetch({
    path: `/accounts/${accountId}`,
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  return await response.json();
}

export async function getAccountTransfers(
  accountId: string,
  nextUrl?: string
): Promise<TransferPage> {
  const response = await apiFetch({
    path: nextUrl || `/accounts/${accountId}/transfers?limit=25`,
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  return await response.json();
}

export async function getAccountLoans(
  accountId: string,
): Promise<Account[]> {
  const response = await apiFetch({
    path: `/accounts/${accountId}/loans`,
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  return response.json();
}
