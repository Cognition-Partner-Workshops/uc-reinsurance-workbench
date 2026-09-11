repo: Cognition-Partner-Workshops/uc-reinsurance-workbench
branch: main
path: src

## Last sync
date: 2026-09-11T12:57:32Z

### Updated in this project
- Built 4-screen portal wireframe (Portfolio, Submission queue, Treaty detail, Pricing/Quote) on top of the API domain model
- Pricing screen reproduces PricingCalculator formula and referral rules from ReferralSeed
- Demo data mirrors the deterministic seed (cedents, submissions, treaties, cat model)

## Screen map
| Screen | Repo files |
| --- | --- |
| Portfolio / Dashboard | src/Reinsurance.Data/Setup/SeedData/TreatySeed.cs, CatModelSeed.cs, ReferenceSeed.cs, src/Reinsurance.Services/Treaties/Models/TreatyModel.cs |
| Submission queue | src/Reinsurance.Data/Setup/SeedData/SubmissionSeed.cs, CedentSeed.cs, src/Reinsurance.Services/Submissions/Models/SubmissionListItemModel.cs, src/Reinsurance.Core/Domain/Submissions/SubmissionStatus.cs |
| Treaty detail | src/Reinsurance.Core/Domain/Treaties/*.cs, src/Reinsurance.Core/Domain/Pricing/PricingResult.cs, src/Reinsurance.Services/Treaties/Models/TreatyLayerModel.cs |
| Pricing / Quote | src/Reinsurance.Services/Pricing/PricingCalculator.cs, PricingService.cs, src/Reinsurance.Data/Setup/SeedData/ReferralSeed.cs, docs/INCIDENTS.md |
