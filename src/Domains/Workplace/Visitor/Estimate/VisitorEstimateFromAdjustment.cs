namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Functions of visitor estimate from adjustment.</summary>
public static class VisitorEstimateFromAdjustment
{
    public static Dictionary<string, double> SummarizePendingVisitorAdjustmentsForEstimate(List<PendingVisitorAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<VisitorEstimate> DeriveVisitorEstimatesFromAdjustment(List<PendingVisitorAdjustment> items)
    {
        var outValue = new List<VisitorEstimate>();
        foreach (var item in items)
        {
            var derived = VisitorEstimate.NewVisitorEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitVisitorEstimate(VisitorEstimate item)
    {
        return VisitorOverridePolicy.VisitorOverrideBucket(item.MeasuredValue());
    }
}
