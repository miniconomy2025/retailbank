import { type Account } from "@/models/accounts";
import type { Transfer } from "@/models/transfers";
import { apiFetch } from "@/utils/api";

export async function getAccounts(): Promise<Account[]> {
  const response = await apiFetch({ path: "/accountss", method: "GET" });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  const data = await response.json();
  return data?.items;
}

export async function getAccount(accountId: number): Promise<Account> {
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
  accountId: number
): Promise<Transfer[]> {
  const response = await apiFetch({
    path: `/accounts/${accountId}/transfers`,
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  const data = await response.json();
  return data?.items;
}
