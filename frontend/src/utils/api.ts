const API_BASE_URL = "https://miniconomyretail.za.bz";

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
