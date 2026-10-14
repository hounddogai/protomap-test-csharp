namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Functions of weighted policy override from review.</summary>
public static class WeightedPolicyOverrideFromReview
{
    public static Dictionary<string, double> SummarizeAccrualReviewsForOverride(List<AccrualReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<WeightedPolicyOverride> DeriveWeightedPolicyOverridesFromReview(List<AccrualReview> items)
    {
        var outValue = new List<WeightedPolicyOverride>();
        foreach (var item in items)
        {
            var derived = WeightedPolicyOverride.NewWeightedPolicyOverride(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreWeightedPolicyOverride(WeightedPolicyOverride item)
    {
        return PrimaryAccrualManifestPolicy.PrimaryAccrualManifestWeight(3, true);
    }
}
