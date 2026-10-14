namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Functions of exception review from claim.</summary>
public static class ExceptionReviewFromClaim
{
    public static Dictionary<string, double> SummarizeLegacyPolicyAckClaimsForReview(List<LegacyPolicyAckClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<ExceptionReview> DeriveExceptionReviewsFromClaim(List<LegacyPolicyAckClaim> items)
    {
        var outValue = new List<ExceptionReview>();
        foreach (var item in items)
        {
            var derived = ExceptionReview.NewExceptionReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeExceptionReview(ExceptionReview item)
    {
        return LegacyPolicyAckClaimPolicy.ClassifyLegacyPolicyAckClaim(item.WeightedLoad());
    }
}
