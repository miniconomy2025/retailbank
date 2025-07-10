import type { ReportData } from "@/models/overview";
import { apiFetch } from "@/utils/api";

export async function getReport(): Promise<ReportData> {
  const response = await apiFetch({
    path: "/report",
    method: "GET",
  });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  const data = await response.json();
  return {
    ...data,
    timestamp: new Date().toLocaleTimeString(),
  };
}
