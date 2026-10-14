namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Functions of position summary from quota.</summary>
public static class PositionSummaryFromQuota
{
    public static Dictionary<string, double> SummarizeSpanQuotasForSummary(List<SpanQuota> items)
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

    public static List<PositionSummary> DerivePositionSummariesFromQuota(List<SpanQuota> items)
    {
        var outValue = new List<PositionSummary>();
        foreach (var item in items)
        {
            var derived = PositionSummary.NewPositionSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyPositionSummaryExtra(PositionSummary item)
    {
        return DefaultSpanProfilePolicy.DefaultSpanProfileWeight(6, true);
    }
}
