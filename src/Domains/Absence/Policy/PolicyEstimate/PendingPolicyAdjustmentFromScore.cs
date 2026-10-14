namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Functions of pending policy adjustment from score.</summary>
public static class PendingPolicyAdjustmentFromScore
{
    public static Dictionary<string, double> SummarizeAdaptivePolicyScoresForAdjustment(List<AdaptivePolicyScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PendingPolicyAdjustment> DerivePendingPolicyAdjustmentsFromScore(List<AdaptivePolicyScore> items)
    {
        var outValue = new List<PendingPolicyAdjustment>();
        foreach (var item in items)
        {
            var derived = PendingPolicyAdjustment.NewPendingPolicyAdjustment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePendingPolicyAdjustment(PendingPolicyAdjustment item)
    {
        return AdaptivePolicyScorePolicy.AdaptivePolicyScoreBucket(item.MeasuredValue());
    }
}
