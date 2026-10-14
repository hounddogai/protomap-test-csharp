namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Functions of occupancy template from override.</summary>
public static class OccupancyTemplateFromOverride
{
    public static Dictionary<string, double> SummarizeVisitorOverridesForTemplate(List<VisitorOverride> items)
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

    public static List<OccupancyTemplate> DeriveOccupancyTemplatesFromOverride(List<VisitorOverride> items)
    {
        var outValue = new List<OccupancyTemplate>();
        foreach (var item in items)
        {
            var derived = OccupancyTemplate.NewOccupancyTemplate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeOccupancyTemplate(OccupancyTemplate item)
    {
        return PendingVisitorAdjustmentPolicy.PendingVisitorAdjustmentBucket(item.AdjustedLoad());
    }
}
