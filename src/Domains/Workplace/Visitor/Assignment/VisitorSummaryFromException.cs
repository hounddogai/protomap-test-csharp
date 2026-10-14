namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Functions of visitor summary from exception.</summary>
public static class VisitorSummaryFromException
{
    public static Dictionary<string, double> SummarizeWeightedAccessBadgeExceptionsForSummary(List<WeightedAccessBadgeException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<VisitorSummary> DeriveVisitorSummariesFromException(List<WeightedAccessBadgeException> items)
    {
        var outValue = new List<VisitorSummary>();
        foreach (var item in items)
        {
            var derived = VisitorSummary.NewVisitorSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanVisitorSummary(VisitorSummary item)
    {
        return WeightedAccessBadgeExceptionPolicy.WeightedAccessBadgeExceptionWeight(4, true);
    }
}
