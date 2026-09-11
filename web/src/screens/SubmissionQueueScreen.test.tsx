import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import SubmissionQueueScreen from "./SubmissionQueueScreen";

const submissions = {
    items: [
        {
            id: 1,
            reference: "SUB-2026-0001",
            status: 0,
            cedentId: 1,
            cedentName: "Meridian Mutual",
            cedentRating: "A+",
            brokerName: "Halcyon Re Brokers",
            underwriterName: "Priya Raman",
            receivedOn: "2026-01-14T00:00:00",
            inceptionDate: "2026-01-01T00:00:00",
            expiryDate: "2026-12-31T00:00:00",
            treatyCount: 1,
        },
        {
            id: 2,
            reference: "SUB-2026-0002",
            status: 3,
            cedentId: 2,
            cedentName: "Gulfstream P&C",
            cedentRating: "A",
            brokerName: "Crestline Intermediaries",
            underwriterName: "Tomas Keller",
            receivedOn: "2026-01-15T00:00:00",
            inceptionDate: "2026-01-01T00:00:00",
            expiryDate: "2026-12-31T00:00:00",
            treatyCount: 1,
        },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 2,
    totalPages: 1,
};

describe("SubmissionQueueScreen", () => {
    beforeEach(() => {
        vi.stubGlobal(
            "fetch",
            vi.fn((input: RequestInfo | URL) => {
                const path = String(input);
                const body = path.includes("/submissions?") ? submissions : [];
                return Promise.resolve(new Response(JSON.stringify(body), { status: 200 }));
            }),
        );
    });

    it("filters rows when a status chip is selected", async () => {
        const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
        render(
            <QueryClientProvider client={client}>
                <MemoryRouter>
                    <SubmissionQueueScreen />
                </MemoryRouter>
            </QueryClientProvider>,
        );

        expect(await screen.findByText("SUB-2026-0001")).toBeInTheDocument();
        expect(screen.getByText("SUB-2026-0002")).toBeInTheDocument();
        await userEvent.click(screen.getByRole("button", { name: /Bound 1/ }));
        await waitFor(() => {
            expect(screen.queryByText("SUB-2026-0001")).not.toBeInTheDocument();
        });
        expect(screen.getByText("SUB-2026-0002")).toBeInTheDocument();
    });
});
