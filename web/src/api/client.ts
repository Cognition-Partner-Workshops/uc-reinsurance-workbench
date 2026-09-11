import * as Sentry from "@sentry/react";
import { sentryEnabled } from "../sentry";

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
        headers: {
            Accept: "application/json",
            ...(init?.body ? { "Content-Type": "application/json" } : {}),
        },
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
        const traceId = response.headers.get("X-Trace-Id") ?? data?.traceId;
        const error = new ApiError(
            response.status,
            data?.message ??
                data?.detail ??
                (typeof body === "string" ? body : response.statusText),
            traceId,
        );
        if (response.status >= 500 && sentryEnabled) {
            Sentry.captureException(error, {
                tags: {
                    traceId: traceId ?? "unknown",
                    apiPath: path,
                    httpStatus: String(response.status),
                },
            });
        }
        throw error;
    }
    return body as T;
}
