import { sampleAccounts } from "@/models/accounts";

function wait(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

export async function getAccounts() {
  await wait(1000)
  console.log("HI")
  return sampleAccounts;
}