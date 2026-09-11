export class ApiError extends Error {
  readonly status: number;
  readonly traceId?: string;

  constructor(status: number, message: string, traceId?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.traceId = traceId;
  }
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { Accept: "application/json", ...(init?.body ? { "Content-Type": "application/json" } : {}) },
    ...init,
  });
  const text = await response.text();
  let body: unknown = null;
  try {
    body = text ? JSON.parse(text) : null;
  } catch {
    body = text;
  }
  if (!response.ok) {
    const data = body as { message?: string; detail?: string; traceId?: string } | null;
    throw new ApiError(
      response.status,
      data?.message ?? data?.detail ?? (typeof body === "string" ? body : response.statusText),
      data?.traceId,
    );
  }
  return body as T;
}
