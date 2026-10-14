namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Functions of audit review from entry.</summary>
public static class AuditReviewFromEntry
{
    public static Dictionary<string, double> SummarizeSeasonalPolicyAckEntriesForReview(List<SeasonalPolicyAckEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<AuditReview> DeriveAuditReviewsFromEntry(List<SeasonalPolicyAckEntry> items)
    {
        var outValue = new List<AuditReview>();
        foreach (var item in items)
        {
            var derived = AuditReview.NewAuditReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanAuditReview(AuditReview item)
    {
        return PolicyAckPreferencePolicy.PolicyAckPreferenceBucket(item.EffectiveLoad());
    }
}
