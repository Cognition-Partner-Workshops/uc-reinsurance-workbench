import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "./client";
import type {
    BrokerModel,
    CatModelResultModel,
    CedentModel,
    ExposureRecordModel,
    ExposureSummary,
    PagedList,
    PortfolioModel,
    PricingResultModel,
    ReferralRuleModel,
    SubmissionCreateRequest,
    SubmissionListItemModel,
    SubmissionModel,
    SubmissionStatus,
    TreatyModel,
    UnderwriterModel,
} from "./types";

export function useSubmissions() {
    return useQuery({
        queryKey: ["submissions"],
        queryFn: () => api<PagedList<SubmissionListItemModel>>("/api/submissions?pageSize=100"),
    });
}

export function useSubmission(id: number | undefined) {
    return useQuery({
        queryKey: ["submission", id],
        queryFn: () => api<SubmissionModel>(`/api/submissions/${id}`),
        enabled: id !== undefined,
    });
}

export function useTreaty(id: number | undefined) {
    return useQuery({
        queryKey: ["treaty", id],
        queryFn: () => api<TreatyModel>(`/api/treaties/${id}`),
        enabled: id !== undefined,
    });
}

export function useTreatyPricing(treatyId: number | undefined) {
    return useQuery({
        queryKey: ["pricing", treatyId],
        queryFn: () => api<PricingResultModel[]>(`/api/treaties/${treatyId}/pricing`),
        enabled: treatyId !== undefined,
    });
}

export function useCatModel(submissionId: number | undefined) {
    return useQuery({
        queryKey: ["cat-model", submissionId],
        queryFn: () => api<CatModelResultModel[]>(`/api/submissions/${submissionId}/cat-model`),
        enabled: submissionId !== undefined,
    });
}

export function useExposure(submissionId: number | undefined) {
    return useQuery({
        queryKey: ["exposure", submissionId],
        queryFn: () =>
            api<{ summary: ExposureSummary; records: ExposureRecordModel[] }>(
                `/api/submissions/${submissionId}/exposure`,
            ),
        enabled: submissionId !== undefined,
    });
}

export function useReferralRules() {
    return useQuery({
        queryKey: ["referral-rules"],
        queryFn: () => api<ReferralRuleModel[]>("/api/reference/referral-rules"),
    });
}

export function useCedents() {
    return useQuery({ queryKey: ["cedents"], queryFn: () => api<CedentModel[]>("/api/cedents") });
}

export function useBrokers() {
    return useQuery({
        queryKey: ["brokers"],
        queryFn: () => api<BrokerModel[]>("/api/reference/brokers"),
    });
}

export function useUnderwriters() {
    return useQuery({
        queryKey: ["underwriters"],
        queryFn: () => api<UnderwriterModel[]>("/api/reference/underwriters"),
    });
}

export function usePortfolio() {
    return useQuery({
        queryKey: ["portfolio"],
        queryFn: () => api<PortfolioModel>("/api/portfolio"),
    });
}

export function usePriceTreaty() {
    const client = useQueryClient();
    return useMutation({
        mutationFn: (treatyId: number) =>
            api<PricingResultModel[]>(`/api/treaties/${treatyId}/price`, {
                method: "POST",
                body: "{}",
            }),
        onSuccess: (_, treatyId) => {
            client.invalidateQueries({ queryKey: ["pricing", treatyId] });
            client.invalidateQueries({ queryKey: ["portfolio"] });
        },
    });
}

export function useBindTreaty() {
    const client = useQueryClient();
    return useMutation({
        mutationFn: (treatyId: number) =>
            api<TreatyModel>(`/api/treaties/${treatyId}/bind`, { method: "POST", body: "{}" }),
        onSuccess: (treaty) => {
            client.invalidateQueries({ queryKey: ["treaty", treaty.id] });
            client.invalidateQueries({ queryKey: ["submissions"] });
            client.invalidateQueries({ queryKey: ["portfolio"] });
        },
    });
}

export function useTransitionSubmission() {
    const client = useQueryClient();
    return useMutation({
        mutationFn: ({ id, status }: { id: number; status: SubmissionStatus }) =>
            api<SubmissionModel>(`/api/submissions/${id}/transition`, {
                method: "POST",
                body: JSON.stringify({ status }),
            }),
        onSuccess: (submission) => {
            client.invalidateQueries({ queryKey: ["submission", submission.id] });
            client.invalidateQueries({ queryKey: ["submissions"] });
            client.invalidateQueries({ queryKey: ["portfolio"] });
        },
    });
}

export function useCreateSubmission() {
    const client = useQueryClient();
    return useMutation({
        mutationFn: (body: SubmissionCreateRequest) =>
            api<SubmissionModel>("/api/submissions", {
                method: "POST",
                body: JSON.stringify(body),
            }),
        onSuccess: () => {
            client.invalidateQueries({ queryKey: ["submissions"] });
            client.invalidateQueries({ queryKey: ["portfolio"] });
        },
    });
}
