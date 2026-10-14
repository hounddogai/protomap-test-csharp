namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Functions of access badge adjustment from template.</summary>
public static class AccessBadgeAdjustmentFromTemplate
{
    public static Dictionary<string, double> SummarizeVisitorTemplatesForAdjustment(List<VisitorTemplate> items)
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

    public static List<AccessBadgeAdjustment> DeriveAccessBadgeAdjustmentsFromTemplate(List<VisitorTemplate> items)
    {
        var outValue = new List<AccessBadgeAdjustment>();
        foreach (var item in items)
        {
            var derived = AccessBadgeAdjustment.NewAccessBadgeAdjustment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAccessBadgeAdjustment(AccessBadgeAdjustment item)
    {
        return VisitorSummaryPolicy.ClassifyVisitorSummary(item.WeightedTotal());
    }
}
