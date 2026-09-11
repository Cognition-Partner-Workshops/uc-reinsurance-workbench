import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import TreatyDetailScreen from "./TreatyDetailScreen";

const submission = {
    id: 1,
    reference: "SUB-2026-0010",
    status: 4,
    cedentId: 6,
    cedentName: "Sakura General",
    cedentRating: "B+",
    brokerName: "Halcyon Re Brokers",
    underwriterName: "Priya Raman",
    receivedOn: "2026-01-14T00:00:00",
    inceptionDate: "2026-01-01T00:00:00",
    expiryDate: "2026-12-31T00:00:00",
    notes: "Property catastrophe placement submission.",
    treaties: [],
};

const catModel = [
    {
        id: 1,
        regionId: null,
        perilId: null,
        modelVendor: "RMS",
        modelVersion: "v23",
        aal: 9000000,
        expectedLoss: 7500000,
        pmL50: 105000000,
        pmL100: 195000000,
        pmL250: 300000000,
        runOn: "2026-02-15T00:00:00",
    },
];

const cedents = [
    {
        id: 6,
        name: "Sakura General",
        code: "CED-006",
        country: "JP",
        rating: "B+",
        active: true,
    },
];

function createClient(treaty: object) {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    client.setQueryData(["treaty", 10], treaty);
    client.setQueryData(["submission", 1], submission);
    client.setQueryData(["cat-model", 1], catModel);
    client.setQueryData(["pricing", 10], []);
    client.setQueryData(["cedents"], cedents);
    return client;
}

function renderTreaty(treaty: object) {
    return render(
        <QueryClientProvider client={createClient(treaty)}>
            <MemoryRouter initialEntries={["/treaties/10"]}>
                <Routes>
                    <Route path="/treaties/:id" element={<TreatyDetailScreen />} />
                </Routes>
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("TreatyDetailScreen (planted incident)", () => {
    beforeEach(() => {
        vi.stubGlobal(
            "fetch",
            vi.fn((input: RequestInfo | URL) => {
                const path = String(input);
                if (path === "/api/treaties/10") {
                    return Promise.resolve(new Response(JSON.stringify({}), { status: 200 }));
                }
                if (path === "/api/submissions/1") {
                    return Promise.resolve(
                        new Response(JSON.stringify(submission), { status: 200 }),
                    );
                }
                if (path === "/api/submissions/1/cat-model") {
                    return Promise.resolve(new Response(JSON.stringify(catModel), { status: 200 }));
                }
                if (path === "/api/treaties/10/pricing") {
                    return Promise.resolve(new Response("[]", { status: 200 }));
                }
                return Promise.resolve(new Response(JSON.stringify(cedents), { status: 200 }));
            }),
        );
    });

    it("renders the treaty name and exhaustion point for layered treaties", () => {
        renderTreaty({
            id: 10,
            name: "Quota Share Treaty",
            type: 2,
            status: 1,
            currency: "USD",
            submissionId: 1,
            inceptionDate: "2026-01-01T00:00:00",
            expiryDate: "2026-12-31T00:00:00",
            layers: [
                {
                    id: 1,
                    layerNumber: 1,
                    limit: 25000000,
                    attachment: 25000000,
                    reinstatements: 1,
                    reinstatementPremiumPct: 0.5,
                    sharePct: 1,
                    currency: "USD",
                },
                {
                    id: 2,
                    layerNumber: 2,
                    limit: 50000000,
                    attachment: 50000000,
                    reinstatements: 1,
                    reinstatementPremiumPct: 0.5,
                    sharePct: 1,
                    currency: "USD",
                },
            ],
        });

        expect(screen.getByRole("heading", { name: "Quota Share Treaty" })).toBeInTheDocument();
        expect(screen.getByText("Exhaustion point")).toBeInTheDocument();
    });

    it("renders a treaty with no layers without an exhaustion point", () => {
        renderTreaty({
            id: 10,
            name: "Quota Share Treaty",
            type: 2,
            status: 1,
            currency: "USD",
            submissionId: 1,
            inceptionDate: "2026-01-01T00:00:00",
            expiryDate: "2026-12-31T00:00:00",
            layers: [],
        });

        expect(screen.getByRole("heading", { name: "Quota Share Treaty" })).toBeInTheDocument();
        const exhaustion = screen.getByText("Exhaustion point").closest(".stat-card");
        expect(exhaustion).not.toBeNull();
        expect(exhaustion!.querySelector("strong")).toHaveTextContent("—");
        expect(
            screen.getByText("Total limit").closest(".stat-card")!.querySelector("strong"),
        ).toHaveTextContent("$0.0M");
    });
});
