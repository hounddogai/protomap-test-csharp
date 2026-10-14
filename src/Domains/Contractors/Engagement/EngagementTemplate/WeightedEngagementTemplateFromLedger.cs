namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Functions of weighted engagement template from ledger.</summary>
public static class WeightedEngagementTemplateFromLedger
{
    public static Dictionary<string, double> SummarizeRateLedgersForTemplate(List<RateLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<WeightedEngagementTemplate> DeriveWeightedEngagementTemplatesFromLedger(List<RateLedger> items)
    {
        var outValue = new List<WeightedEngagementTemplate>();
        foreach (var item in items)
        {
            var derived = WeightedEngagementTemplate.NewWeightedEngagementTemplate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanWeightedEngagementTemplate(WeightedEngagementTemplate item)
    {
        return PrimaryRateTierPolicy.PrimaryRateTierBucket(item.ProjectedExposure());
    }
}
