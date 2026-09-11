import { Link, useLocation } from "react-router-dom";
import { useSubmissions, useUnderwriters } from "../api/queries";
import { SubmissionStatus } from "../api/types";

interface LayoutProps {
  children: React.ReactNode;
  lastTreatyId?: number;
}

export default function Layout({ children, lastTreatyId }: LayoutProps) {
  const location = useLocation();
  const submissions = useSubmissions();
  const underwriters = useUnderwriters();
  const items = submissions.data?.items ?? [];
  const queueCount = items.filter(
    (item) => item.status === SubmissionStatus.Received || item.status === SubmissionStatus.InReview,
  ).length;
  const portfolioCount = items.filter((item) => item.status !== SubmissionStatus.Declined && item.treatyCount > 0)
    .length;
  const current = underwriters.data?.find((item) => item.name === "Tomas Keller");

  const active = (path: string) => (path === "/" ? location.pathname === "/" : location.pathname.startsWith(path));

  return (
    <div className="app-shell">
      <nav className="sidebar">
        <div className="brand">
          <div className="brand-mark" />
          <div>
            <div className="brand-title">Reinsurance Workbench</div>
            <div className="muted small">Underwriting portal</div>
          </div>
        </div>
        <Link className={`nav-item ${active("/") ? "active" : ""}`} to="/">
          <span className="nav-dot" /> Portfolio <span className="nav-count">{portfolioCount}</span>
        </Link>
        <Link className={`nav-item ${active("/submissions") ? "active" : ""}`} to="/submissions">
          <span className="nav-dot" /> Submissions <span className="nav-count">{queueCount}</span>
        </Link>
        <Link
          className={`nav-item ${active("/treaties/") && !location.pathname.endsWith("/pricing") ? "active" : ""}`}
          to={lastTreatyId ? `/treaties/${lastTreatyId}` : "/"}
        >
          <span className="nav-dot" /> Treaty detail
        </Link>
        <Link
          className={`nav-item ${location.pathname.endsWith("/pricing") ? "active" : ""}`}
          to={lastTreatyId ? `/treaties/${lastTreatyId}/pricing` : "/"}
        >
          <span className="nav-dot" /> Pricing
        </Link>
        <div className="sidebar-footer">
          <div className="avatar">TK</div>
          <div>
            <div className="footer-name">Tomas Keller</div>
            <div className="muted small">
              Authority {current ? `$${(current.authorityLimit / 1e6).toFixed(1)}M` : "—"}
            </div>
          </div>
        </div>
      </nav>
      <main className="main-content">{children}</main>
    </div>
  );
}
