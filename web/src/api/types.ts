export enum SubmissionStatus {
    Received = 0,
    InReview = 1,
    Quoted = 2,
    Bound = 3,
    Declined = 4,
    Withdrawn = 5,
}

export enum TreatyStatus {
    Draft = 0,
    Quoted = 1,
    Bound = 2,
    Expired = 3,
}

export enum TreatyType {
    PropertyCatXoL = 0,
    PerRiskXoL = 1,
    QuotaShare = 2,
    AggregateXoL = 3,
}

export const submissionStatusLabels: Record<number, string> = {
    [SubmissionStatus.Received]: "Received",
    [SubmissionStatus.InReview]: "In review",
    [SubmissionStatus.Quoted]: "Quoted",
    [SubmissionStatus.Bound]: "Bound",
    [SubmissionStatus.Declined]: "Declined",
    [SubmissionStatus.Withdrawn]: "Withdrawn",
};

export const treatyStatusLabels: Record<number, string> = {
    [TreatyStatus.Draft]: "Draft",
    [TreatyStatus.Quoted]: "Quoted",
    [TreatyStatus.Bound]: "Bound",
    [TreatyStatus.Expired]: "Expired",
};

export const treatyTypeLabels: Record<number, string> = {
    [TreatyType.PropertyCatXoL]: "Property Cat XoL",
    [TreatyType.PerRiskXoL]: "Per Risk XoL",
    [TreatyType.QuotaShare]: "Quota Share",
    [TreatyType.AggregateXoL]: "Aggregate XoL",
};

export interface SubmissionListItemModel {
    id: number;
    reference: string;
    status: SubmissionStatus;
    cedentId: number;
    cedentName: string;
    cedentRating: string;
    brokerName: string;
    underwriterName: string;
    receivedOn: string;
    inceptionDate: string;
    expiryDate: string;
    treatyCount: number;
}

export interface SubmissionModel extends SubmissionListItemModel {
    brokerId?: number;
    underwriterId?: number;
    underwriterAuthorityLimit?: number;
    notes?: string;
    treaties: TreatyModel[];
}

export interface TreatyModel {
    id: number;
    name: string;
    type: TreatyType;
    status: TreatyStatus;
    currency: string;
    submissionId: number;
    inceptionDate: string;
    expiryDate: string;
    layers: TreatyLayerModel[];
}

export interface TreatyLayerModel {
    id: number;
    layerNumber: number;
    limit: number;
    attachment: number;
    reinstatements: number;
    reinstatementPremiumPct: number;
    sharePct: number;
    currency: string;
}

export interface PricingResultModel {
    treatyLayerId: number;
    technicalPremium: number;
    expectedLoss: number;
    expenseLoad: number;
    riskLoad: number;
    rateOnLine: number;
    lossCostPct: number;
    profitMarginPct: number;
    referralRequired: boolean;
    referralReasons: string;
    ourShareLine: number;
    calculatedOn?: string;
}

export interface CatModelResultModel {
    id: number;
    regionId?: number | null;
    perilId?: number | null;
    modelVendor: string;
    modelVersion: string;
    aal: number;
    expectedLoss: number;
    pmL50: number;
    pmL100: number;
    pmL250: number;
    runOn: string;
}

export interface ExposureBreakdown {
    regionCode?: string;
    regionName?: string;
    perilCode?: string;
    perilName?: string;
    tiv: number;
    sharePct: number;
}

export interface ExposureSummary {
    totalTiv: number;
    byRegion: ExposureBreakdown[];
    byPeril: ExposureBreakdown[];
    concentration: number;
    pmlToTivRatio: number;
    riskCount: number;
}

export interface PortfolioModel {
    submissions: SubmissionModel[];
    treatyPricing: { treatyId: number; results: PricingResultModel[] }[];
    catModels: { submissionId: number; results: CatModelResultModel[] }[];
    exposures: { submissionId: number; summary: ExposureSummary }[];
}

export interface ExposureRecordModel {
    id: number;
    regionCode: string;
    regionName: string;
    perilCode: string;
    perilName: string;
    totalInsuredValue: number;
    riskCount: number;
    averageDeductiblePct: number;
}

export interface CedentModel {
    id: number;
    name: string;
    code: string;
    country: string;
    rating: string;
    active: boolean;
}

export interface BrokerModel {
    id: number;
    name: string;
    code?: string;
    active: boolean;
}

export interface UnderwriterModel {
    id: number;
    name: string;
    email: string;
    authorityLimit: number;
    active: boolean;
}

export interface ReferralRuleModel {
    id: number;
    code: string;
    description: string;
    active: boolean;
    kind: number;
    threshold?: number;
    textValue?: string;
}

export interface PagedList<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface SubmissionCreateRequest {
    cedentId: number;
    brokerId?: number;
    underwriterId?: number;
    inceptionDate: string;
    expiryDate: string;
    notes?: string;
}
