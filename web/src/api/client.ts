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
        const data = body && typeof body === "object"
            ? body as { message?: unknown; detail?: unknown; traceId?: unknown }
            : null;
        const traceId = response.headers.get("X-Trace-Id") ??
            (typeof data?.traceId === "string" ? data.traceId : undefined);
        const message = [data?.message, data?.detail].find(
            (value): value is string => typeof value === "string" && value.trim().length > 0,
        );
        const error = new ApiError(
            response.status,
            message ?? (response.status >= 500
                ? "The server could not complete the request. Please try again."
                : "The request failed. Please try again."),
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
