const API_BASE_URL = "http://localhost:5173";

type Props = {
  method: string;
  path: string;
  body?: string;
};

// TODO: Add whatever auth we go with here
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
