import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { api } from "../api/client";
import {
    useBrokers,
    useCedents,
    useCreateSubmission,
    useSubmissions,
    useUnderwriters,
} from "../api/queries";
import type {
    BrokerModel,
    CedentModel,
    SubmissionCreateRequest,
    SubmissionListItemModel,
    SubmissionModel,
    UnderwriterModel,
} from "../api/types";
import { SubmissionStatus, submissionStatusLabels } from "../api/types";
import { fmtDate } from "../domain/format";
import { ErrorBanner, PageHeader, StatusPill } from "../components/Ui";

const filters = [
    undefined,
    SubmissionStatus.Received,
    SubmissionStatus.InReview,
    SubmissionStatus.Quoted,
    SubmissionStatus.Bound,
] as const;

export default function SubmissionQueueScreen() {
    const [filter, setFilter] = useState<SubmissionStatus | undefined>();
    const [search, setSearch] = useState("");
    const [showNew, setShowNew] = useState(false);
    const submissions = useSubmissions();
    const cedents = useCedents();
    const brokers = useBrokers();
    const underwriters = useUnderwriters();
    const create = useCreateSubmission();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const rows = (submissions.data?.items ?? []).filter(
        (row) =>
            (!filter || row.status === filter) &&
            (!search ||
                `${row.reference} ${row.cedentName}`.toLowerCase().includes(search.toLowerCase())),
    );
    const openSubmission = async (row: SubmissionListItemModel) => {
        if (!row.treatyCount) {
            navigate(`/submissions/${row.id}`);
            return;
        }
        const detail = await queryClient.fetchQuery({
            queryKey: ["submission", row.id],
            queryFn: () => api<SubmissionModel>(`/api/submissions/${row.id}`),
        });
        navigate(
            detail.treaties[0] ? `/treaties/${detail.treaties[0].id}` : `/submissions/${row.id}`,
        );
    };
    if (submissions.error) return <ErrorBanner error={submissions.error} />;
    return (
        <>
            <PageHeader eyebrow="Submissions" title="Submission queue">
                <button className="primary-button" onClick={() => setShowNew(true)}>
                    + New submission
                </button>
            </PageHeader>
            <div className="queue-toolbar">
                <input
                    aria-label="Search submissions"
                    placeholder="Search reference or cedent…"
                    value={search}
                    onChange={(event) => setSearch(event.target.value)}
                />
                <div className="filter-row">
                    {filters.map((status) => (
                        <button
                            key={String(status)}
                            className={`filter-chip ${filter === status ? "selected" : ""}`}
                            onClick={() => setFilter(status)}
                        >
                            {status === undefined ? "All" : submissionStatusLabels[status]}{" "}
                            <span>
                                {status === undefined
                                    ? (submissions.data?.totalCount ?? 0)
                                    : (submissions.data?.items.filter(
                                          (item) => item.status === status,
                                      ).length ?? 0)}
                            </span>
                        </button>
                    ))}
                </div>
            </div>
            <section className="card table-card">
                <div className="table-head queue-grid">
                    <span>Reference</span>
                    <span>Cedent</span>
                    <span>Rating</span>
                    <span>Broker</span>
                    <span>Underwriter</span>
                    <span>Received</span>
                    <span>Status</span>
                </div>
                {rows.map((row) => (
                    <button
                        className="table-row queue-grid"
                        key={row.id}
                        onClick={() => void openSubmission(row)}
                    >
                        <span className="mono">{row.reference}</span>
                        <span>{row.cedentName}</span>
                        <span>{row.cedentRating}</span>
                        <span>{row.brokerName}</span>
                        <span>{row.underwriterName}</span>
                        <span>{fmtDate(row.receivedOn)}</span>
                        <StatusPill status={row.status} />
                    </button>
                ))}
                {!rows.length && (
                    <div className="empty-state">No submissions match this filter.</div>
                )}
            </section>
            {showNew && (
                <NewSubmissionModal
                    cedents={cedents.data ?? []}
                    brokers={brokers.data ?? []}
                    underwriters={underwriters.data ?? []}
                    loading={create.isPending}
                    onClose={() => setShowNew(false)}
                    onSubmit={(body) =>
                        create.mutate(body, {
                            onSuccess: (submission) => navigate(`/submissions/${submission.id}`),
                        })
                    }
                />
            )}
        </>
    );
}

function NewSubmissionModal({
    cedents,
    brokers,
    underwriters,
    loading,
    onClose,
    onSubmit,
}: {
    cedents: CedentModel[];
    brokers: BrokerModel[];
    underwriters: UnderwriterModel[];
    loading: boolean;
    onClose: () => void;
    onSubmit: (body: SubmissionCreateRequest) => void;
}) {
    const [cedentId, setCedentId] = useState(cedents[0]?.id ?? 0);
    const [brokerId, setBrokerId] = useState(brokers[0]?.id);
    const [underwriterId, setUnderwriterId] = useState(underwriters[0]?.id);
    const [inceptionDate, setInceptionDate] = useState("2026-01-01");
    const [expiryDate, setExpiryDate] = useState("2026-12-31");
    const [notes, setNotes] = useState("");
    return (
        <div className="modal-backdrop">
            <form
                className="modal card"
                onSubmit={(event) => {
                    event.preventDefault();
                    onSubmit({
                        cedentId,
                        brokerId,
                        underwriterId,
                        inceptionDate,
                        expiryDate,
                        notes,
                    });
                }}
            >
                <div className="section-heading">
                    <h2>New submission</h2>
                    <button type="button" className="icon-button" onClick={onClose}>
                        ×
                    </button>
                </div>
                <label>
                    Cedent
                    <select
                        value={cedentId}
                        onChange={(event) => setCedentId(Number(event.target.value))}
                    >
                        {cedents.map((item) => (
                            <option key={item.id} value={item.id}>
                                {item.name}
                            </option>
                        ))}
                    </select>
                </label>
                <div className="form-grid">
                    <label>
                        Inception
                        <input
                            type="date"
                            value={inceptionDate}
                            onChange={(event) => setInceptionDate(event.target.value)}
                        />
                    </label>
                    <label>
                        Expiry
                        <input
                            type="date"
                            value={expiryDate}
                            onChange={(event) => setExpiryDate(event.target.value)}
                        />
                    </label>
                </div>
                <label>
                    Broker
                    <select
                        value={brokerId}
                        onChange={(event) => setBrokerId(Number(event.target.value))}
                    >
                        {brokers.map((item) => (
                            <option key={item.id} value={item.id}>
                                {item.name}
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Underwriter
                    <select
                        value={underwriterId}
                        onChange={(event) => setUnderwriterId(Number(event.target.value))}
                    >
                        {underwriters.map((item) => (
                            <option key={item.id} value={item.id}>
                                {item.name}
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Notes
                    <textarea value={notes} onChange={(event) => setNotes(event.target.value)} />
                </label>
                <div className="modal-actions">
                    <button type="button" className="secondary-button" onClick={onClose}>
                        Cancel
                    </button>
                    <button className="primary-button" disabled={loading}>
                        Create submission
                    </button>
                </div>
            </form>
        </div>
    );
}
