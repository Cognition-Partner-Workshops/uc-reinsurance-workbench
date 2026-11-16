import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
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

    it("filters rows when the Received chip is selected", async () => {
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
        const received = screen.getByRole("button", { name: /Received 1/ });
        await userEvent.click(received);
        expect(received).toHaveClass("selected");
        await waitFor(() => {
            expect(screen.getByText("SUB-2026-0001")).toBeInTheDocument();
        });
        expect(screen.queryByText("SUB-2026-0002")).not.toBeInTheDocument();
    });

    describe("renewal badge", () => {
        const item = (id: number, status: number, expiryDate: string) => ({
            ...submissions.items[0],
            id,
            reference: `SUB-2026-000${id}`,
            status,
            expiryDate,
        });
        const renderQueue = async (items: ReturnType<typeof item>[]) => {
            vi.stubGlobal(
                "fetch",
                vi.fn((input: RequestInfo | URL) => {
                    const body = String(input).includes("/submissions?")
                        ? { ...submissions, items, totalCount: items.length }
                        : [];
                    return Promise.resolve(new Response(JSON.stringify(body), { status: 200 }));
                }),
            );
            const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
            render(
                <QueryClientProvider client={client}>
                    <MemoryRouter>
                        <SubmissionQueueScreen />
                    </MemoryRouter>
                </QueryClientProvider>,
            );
            await screen.findByText(items[0].reference);
        };
        const rowFor = (reference: string) => screen.getByText(reference).closest("button")!;

        beforeEach(() => {
            vi.useFakeTimers({ toFake: ["Date"] });
            vi.setSystemTime(new Date(2026, 10, 16, 9, 0));
        });

        afterEach(() => {
            vi.useRealTimers();
        });

        it("shows an amber badge with the days remaining", async () => {
            await renderQueue([item(1, 0, "2026-12-31T00:00:00")]);

            const badge = screen.getByText("Renewal due in 45 days");
            expect(badge).toHaveClass("renewal-amber");
            expect(rowFor("SUB-2026-0001")).toContainElement(badge);
        });

        it("turns red at 14 days or fewer and shows Lapsed after expiry", async () => {
            await renderQueue([
                item(1, 1, "2026-11-30T00:00:00"),
                item(2, 2, "2026-11-15T00:00:00"),
            ]);

            expect(screen.getByText("Renewal due in 14 days")).toHaveClass("renewal-red");
            expect(screen.getByText("Lapsed")).toHaveClass("renewal-lapsed");
            expect(rowFor("SUB-2026-0002")).toContainElement(screen.getByText("Lapsed"));
        });

        it("shows no badge for Bound, Declined, or far-off expiries", async () => {
            await renderQueue([
                item(1, 3, "2026-12-31T00:00:00"),
                item(2, 4, "2026-11-01T00:00:00"),
                item(3, 0, "2027-06-30T00:00:00"),
            ]);

            expect(screen.queryByText(/Renewal due/)).not.toBeInTheDocument();
            expect(screen.queryByText("Lapsed")).not.toBeInTheDocument();
        });
    });
});
