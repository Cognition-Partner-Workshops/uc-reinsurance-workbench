import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import PortfolioScreen from "./PortfolioScreen";

function layer(id: number, limit: number, attachment: number) {
    return {
        id,
        layerNumber: 1,
        limit,
        attachment,
        reinstatements: 1,
        reinstatementPremiumPct: 1,
        sharePct: 1,
        currency: "USD",
    };
}

function submission(id: number, cedentName: string, treatyStatus: number, limit: number) {
    return {
        id,
        reference: `SUB-2026-000${id}`,
        status: 2,
        cedentId: id,
        cedentName,
        cedentRating: "A",
        brokerName: "Halcyon Re Brokers",
        underwriterName: "Priya Raman",
        receivedOn: "2026-01-14T00:00:00",
        inceptionDate: "2026-01-01T00:00:00",
        expiryDate: "2026-12-31T00:00:00",
        treatyCount: 1,
        treaties: [
            {
                id: id * 10,
                name: `${cedentName} Cat XoL 2026`,
                type: 0,
                status: treatyStatus,
                currency: "USD",
                submissionId: id,
                inceptionDate: "2026-01-01T00:00:00",
                expiryDate: "2026-12-31T00:00:00",
                layers: [layer(id * 100, limit, limit)],
            },
        ],
    };
}

function pricing(treatyLayerId: number, technicalPremium: number) {
    return {
        treatyLayerId,
        technicalPremium,
        expectedLoss: technicalPremium / 2,
        expenseLoad: 0,
        riskLoad: 0,
        rateOnLine: 0.1,
        lossCostPct: 0.05,
        profitMarginPct: 0.1,
        referralRequired: false,
        referralReasons: "",
        ourShareLine: technicalPremium,
    };
}

function catModel(id: number, pmL250: number) {
    return {
        id,
        submissionId: id,
        regionId: null,
        perilId: null,
        modelVendor: "RMS",
        modelVersion: "v23",
        aal: pmL250 / 20,
        expectedLoss: pmL250 / 25,
        pmL50: pmL250 / 3,
        pmL100: pmL250 / 2,
        pmL250,
        runOn: "2026-02-15T00:00:00",
    };
}

function exposure(perilName: string, tiv: number) {
    return {
        totalTiv: tiv,
        byRegion: [],
        byPeril: [{ perilCode: perilName.slice(0, 2).toUpperCase(), perilName, tiv, sharePct: 1 }],
        concentration: 1,
        pmlToTivRatio: 0.1,
        riskCount: 10,
    };
}

const submissions = [
    submission(1, "Meridian Mutual", 2, 50_000_000),
    submission(2, "Gulfstream P&C", 1, 25_000_000),
];

const portfolio = {
    submissions,
    treatyPricing: [
        { treatyId: 10, results: [pricing(100, 4_000_000)] },
        { treatyId: 20, results: [pricing(200, 1_500_000)] },
    ],
    catModels: [
        { submissionId: 1, results: [catModel(1, 120_000_000)] },
        { submissionId: 2, results: [catModel(2, 80_000_000)] },
    ],
    exposures: [
        { submissionId: 1, summary: exposure("Windstorm", 900_000_000) },
        { submissionId: 2, summary: exposure("Earthquake", 400_000_000) },
    ],
};

function json(body: unknown) {
    return Promise.resolve(new Response(JSON.stringify(body), { status: 200 }));
}

function renderPortfolio() {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    return render(
        <QueryClientProvider client={client}>
            <MemoryRouter>
                <PortfolioScreen onTreaty={() => {}} />
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("PortfolioScreen", () => {
    let fetchMock: ReturnType<typeof vi.fn>;

    beforeEach(() => {
        fetchMock = vi.fn((input: RequestInfo | URL) => {
            const path = String(input);
            if (path === "/api/portfolio") {
                return json(portfolio);
            }
            if (path.startsWith("/api/submissions?")) {
                return json({
                    items: submissions.map(({ treaties: _treaties, ...item }) => item),
                    page: 1,
                    pageSize: 100,
                    totalCount: submissions.length,
                    totalPages: 1,
                });
            }
            const detail = path.match(/^\/api\/submissions\/(\d+)$/);
            if (detail) {
                return json(submissions.find((item) => item.id === Number(detail[1])));
            }
            const catModels = path.match(/^\/api\/submissions\/(\d+)\/cat-model$/);
            if (catModels) {
                return json(
                    portfolio.catModels.find((row) => row.submissionId === Number(catModels[1]))
                        ?.results ?? [],
                );
            }
            const exposures = path.match(/^\/api\/submissions\/(\d+)\/exposure$/);
            if (exposures) {
                const summary = portfolio.exposures.find(
                    (row) => row.submissionId === Number(exposures[1]),
                )?.summary;
                return json({ summary, records: [] });
            }
            const treatyPricing = path.match(/^\/api\/treaties\/(\d+)\/pricing$/);
            if (treatyPricing) {
                return json(
                    portfolio.treatyPricing.find(
                        (row) => row.treatyId === Number(treatyPricing[1]),
                    )?.results ?? [],
                );
            }
            return Promise.resolve(new Response("Not found", { status: 404 }));
        });
        vi.stubGlobal("fetch", fetchMock);
    });

    it("loads the dashboard with a single portfolio request instead of one call per submission", async () => {
        renderPortfolio();

        expect(await screen.findByText("Gulfstream P&C Cat XoL 2026")).toBeInTheDocument();
        expect(screen.getAllByText("Meridian Mutual Cat XoL 2026")).not.toHaveLength(0);

        const requested = fetchMock.mock.calls.map(([input]) => String(input));
        expect(requested).toEqual(["/api/portfolio"]);
        expect(requested.filter((path) => /^\/api\/submissions\/\d+/.test(path))).toHaveLength(0);
        expect(requested.filter((path) => /^\/api\/treaties\/\d+\/pricing$/.test(path))).toHaveLength(
            0,
        );
    });

    it("aggregates premium, PML and exposure from the portfolio response", async () => {
        renderPortfolio();

        expect(await screen.findByText("Gulfstream P&C Cat XoL 2026")).toBeInTheDocument();

        const boundPremium = screen.getByText("Bound premium").parentElement;
        expect(boundPremium).toHaveTextContent("$4.0M");
        expect(boundPremium).toHaveTextContent("1 bound treaties");
        const pipeline = screen.getByText("Quoted pipeline").parentElement;
        expect(pipeline).toHaveTextContent("$1.5M");
        expect(pipeline).toHaveTextContent("1 quoted treaties");
        expect(screen.getByText("Portfolio PML 250").parentElement).toHaveTextContent("$200M");
        expect(screen.getByText("Aggregate limit").parentElement).toHaveTextContent("$75.0M");
        expect(screen.getByText("Windstorm")).toBeInTheDocument();
        expect(screen.getByText("Earthquake")).toBeInTheDocument();
    });
});
