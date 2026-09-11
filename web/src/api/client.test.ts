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
            traceId: "conflict-id",
        });
        expect(Sentry.captureException).not.toHaveBeenCalled();
    });
});
