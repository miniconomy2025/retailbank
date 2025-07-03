import type { Transfer, TransferPage } from "@/models/transfers";
import { apiFetch } from "@/utils/api";

export async function getTransfers(
  nextUrl?: string
): Promise<TransferPage> {
  const response = await apiFetch({
    path: nextUrl || `/transfers?limit=25`,
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  return await response.json();
}

export async function getTransfer(transferId: number): Promise<Transfer[]> {
  const response = await apiFetch({
    path: `/transfers/${transferId}`,
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  return await response.json();
}
