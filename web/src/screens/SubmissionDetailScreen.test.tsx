import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import SubmissionDetailScreen from "./SubmissionDetailScreen";

const submission = {
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
    notes: "Property catastrophe placement submission.",
    treaties: [],
};

describe("SubmissionDetailScreen", () => {
    beforeEach(() => {
        vi.stubGlobal(
            "fetch",
            vi.fn((input: RequestInfo | URL, init?: RequestInit) => {
                if (init?.method === "POST") {
                    return Promise.resolve(
                        new Response(JSON.stringify({ ...submission, status: 1 }), { status: 200 }),
                    );
                }
                return Promise.resolve(new Response(JSON.stringify(submission), { status: 200 }));
            }),
        );
    });

    it("posts the selected transition status", async () => {
        const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
        render(
            <QueryClientProvider client={client}>
                <MemoryRouter initialEntries={["/submissions/1"]}>
                    <Routes>
                        <Route path="/submissions/:id" element={<SubmissionDetailScreen />} />
                    </Routes>
                </MemoryRouter>
            </QueryClientProvider>,
        );

        const select = await screen.findByRole("combobox");
        await userEvent.selectOptions(select, "5");
        await userEvent.click(screen.getByRole("button", { name: "Transition" }));

        await waitFor(() => {
            expect(vi.mocked(fetch)).toHaveBeenCalledWith(
                "/api/submissions/1/transition",
                expect.objectContaining({
                    method: "POST",
                    body: JSON.stringify({ status: 5 }),
                }),
            );
        });
    });
});
