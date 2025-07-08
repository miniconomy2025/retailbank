import type { Transfer, TransferPage } from "@/models/transfers";
import { apiFetch } from "@/utils/api";

export async function getTransfers(
  nextUrl?: string,
  limit : number  = 25
): Promise<TransferPage> {
  const response = await apiFetch({
    path: nextUrl || `/transfers?limit=${limit}`,
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  return await response.json();
}

export async function getTransfer(transferId: string): Promise<Transfer> {
  const response = await apiFetch({
    path: `/transfers/${transferId}`,
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  return await response.json();
}
