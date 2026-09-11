import { useState } from "react";
import * as Sentry from "@sentry/react";
import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import PortfolioScreen from "./screens/PortfolioScreen";
import SubmissionQueueScreen from "./screens/SubmissionQueueScreen";
import SubmissionDetailScreen from "./screens/SubmissionDetailScreen";
import TreatyDetailScreen from "./screens/TreatyDetailScreen";
import PricingScreen from "./screens/PricingScreen";

const SentryRoutes = Sentry.withSentryReactRouterV6Routing(Routes);

export default function App() {
    const [lastTreatyId, setLastTreatyId] = useState<number>();
    const rememberTreaty = (id: number) => setLastTreatyId(id);
    return (
        <Layout lastTreatyId={lastTreatyId}>
            <SentryRoutes>
                <Route
                    path="/"
                    element={
                        <PortfolioScreen lastTreatyId={lastTreatyId} onTreaty={rememberTreaty} />
                    }
                />
                <Route path="/submissions" element={<SubmissionQueueScreen />} />
                <Route path="/submissions/:id" element={<SubmissionDetailScreen />} />
                <Route path="/treaties/:id" element={<TreatyDetailScreen />} />
                <Route path="/treaties/:id/pricing" element={<PricingScreen />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </SentryRoutes>
        </Layout>
    );
}
