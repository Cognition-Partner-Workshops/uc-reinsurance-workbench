import * as Sentry from "@sentry/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError, api } from "./client";

vi.mock("@sentry/react", () => ({
    captureException: vi.fn(),
}));

vi.mock("../sentry", () => ({
    sentryEnabled: true,
}));

describe("api client Sentry instrumentation", () => {
    beforeEach(() => {
        vi.resetAllMocks();
    });

    it("uses the trace header and captures server errors", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn(() =>
                Promise.resolve(
                    new Response(JSON.stringify({ detail: "server failed", traceId: "body-id" }), {
                        status: 500,
                        headers: { "X-Trace-Id": "header-id" },
                    }),
                ),
            ),
        );

        await expect(api("/api/fail")).rejects.toMatchObject({
            status: 500,
            message: "server failed",
            traceId: "header-id",
        });
        expect(Sentry.captureException).toHaveBeenCalledWith(expect.any(ApiError), {
            tags: {
                traceId: "header-id",
                apiPath: "/api/fail",
                httpStatus: "500",
            },
        });
    });

    it("does not capture client conflicts", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn(() =>
                Promise.resolve(
                    new Response(JSON.stringify({ detail: "conflict", traceId: "conflict-id" }), {
                        status: 409,
                    }),
                ),
            ),
        );

        await expect(api("/api/conflict")).rejects.toMatchObject({
            status: 409,
            message: "conflict",
            traceId: "conflict-id",
        });
        expect(Sentry.captureException).not.toHaveBeenCalled();
    });

    it.each([
        ["<?xml version=\"1.0\"?><html><body>Runtime Error</body></html>", "text/html"],
        ["<html><body>Bad Gateway</body></html>", "application/json"],
        ["upstream failed", "text/plain"],
        ["", "text/html"],
        [JSON.stringify({ message: { internal: "details" }, detail: 123, traceId: 123 }), "application/json"],
    ])("uses a safe fallback for an unstructured error: %s", async (body, contentType) => {
        vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(body, {
            status: 500,
            headers: { "Content-Type": contentType, "X-Trace-Id": "header-id" },
        })));
        await expect(api("/api/fail")).rejects.toMatchObject({
            status: 500,
            message: "The server could not complete the request. Please try again.",
            traceId: "header-id",
        });
        expect(Sentry.captureException).toHaveBeenCalledWith(
            expect.objectContaining({ message: "The server could not complete the request. Please try again." }),
            expect.any(Object),
        );
    });

    it("uses a safe fallback for an empty client error", async () => {
        vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("", { status: 400 })));
        await expect(api("/api/fail")).rejects.toMatchObject({ status: 400, message: "The request failed. Please try again." });
        expect(Sentry.captureException).not.toHaveBeenCalled();
    });

    it("returns successful JSON unchanged", async () => {
        vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ items: [] }))));
        await expect(api("/api/items")).resolves.toEqual({ items: [] });
        expect(Sentry.captureException).not.toHaveBeenCalled();
    });
});
