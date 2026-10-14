namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Functions of batched bonus pool review from claim.</summary>
public static class BatchedBonusPoolReviewFromClaim
{
    public static Dictionary<string, double> SummarizeFallbackMeritCycleClaimsForReview(List<FallbackMeritCycleClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BatchedBonusPoolReview> DeriveBatchedBonusPoolReviewsFromClaim(List<FallbackMeritCycleClaim> items)
    {
        var outValue = new List<BatchedBonusPoolReview>();
        foreach (var item in items)
        {
            var derived = BatchedBonusPoolReview.NewBatchedBonusPoolReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBatchedBonusPoolReview(BatchedBonusPoolReview item)
    {
        return FallbackMeritCycleClaimPolicy.FallbackMeritCycleClaimBucket(item.EffectiveExposure());
    }
}
