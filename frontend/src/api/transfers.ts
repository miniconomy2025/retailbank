import type { Transfer } from "@/models/transfers";
import { apiFetch } from "@/utils/api";

export async function getTransfers(): Promise<Transfer[]> {
  const response = await apiFetch({
    path: `/transfers`,
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  const data = await response.json();
  return data?.items;
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
