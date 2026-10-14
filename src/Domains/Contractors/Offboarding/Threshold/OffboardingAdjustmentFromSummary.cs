namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Functions of offboarding adjustment from summary.</summary>
public static class OffboardingAdjustmentFromSummary
{
    public static Dictionary<string, double> SummarizeEngagementSummariesForAdjustment(List<EngagementSummary> items)
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

    public static List<OffboardingAdjustment> DeriveOffboardingAdjustmentsFromSummary(List<EngagementSummary> items)
    {
        var outValue = new List<OffboardingAdjustment>();
        foreach (var item in items)
        {
            var derived = OffboardingAdjustment.NewOffboardingAdjustment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankOffboardingAdjustment(OffboardingAdjustment item)
    {
        return EngagementTierPolicy.NormalizeEngagementTierCode(item.Code);
    }
}
