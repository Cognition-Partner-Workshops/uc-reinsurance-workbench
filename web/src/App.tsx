import { useState } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import PortfolioScreen from "./screens/PortfolioScreen";
import SubmissionQueueScreen from "./screens/SubmissionQueueScreen";
import SubmissionDetailScreen from "./screens/SubmissionDetailScreen";
import TreatyDetailScreen from "./screens/TreatyDetailScreen";
import PricingScreen from "./screens/PricingScreen";

export default function App() {
  const [lastTreatyId, setLastTreatyId] = useState<number>();
  const rememberTreaty = (id: number) => setLastTreatyId(id);
  return <Layout lastTreatyId={lastTreatyId}><Routes><Route path="/" element={<PortfolioScreen onTreaty={rememberTreaty} />} /><Route path="/submissions" element={<SubmissionQueueScreen />} /><Route path="/submissions/:id" element={<SubmissionDetailScreen />} /><Route path="/treaties/:id" element={<TreatyDetailScreen />} /><Route path="/treaties/:id/pricing" element={<PricingScreen />} /><Route path="*" element={<Navigate to="/" replace />} /></Routes></Layout>;
}
