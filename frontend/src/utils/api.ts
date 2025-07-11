const API_BASE_URL = "https://retail-bank.projects.bbdgrad.com/api";

type Props = {
  method: string;
  path: string;
  body?: string;
};

export const apiFetch = async ({
  path,
  method,
  body,
}: Props): Promise<Response> => {
  const res = await fetch(API_BASE_URL + path, {
    method: method,
    headers: {},
    body: body,
  });
  return res;
};
