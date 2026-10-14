namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Functions of span summary from estimate.</summary>
public static class SpanSummaryFromEstimate
{
    public static Dictionary<string, double> SummarizeCostModelEstimatesForSummary(List<CostModelEstimate> items)
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

    public static List<SpanSummary> DeriveSpanSummariesFromEstimate(List<CostModelEstimate> items)
    {
        var outValue = new List<SpanSummary>();
        foreach (var item in items)
        {
            var derived = SpanSummary.NewSpanSummary(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateSpanSummary(SpanSummary item)
    {
        return CostModelGroupPolicy.NormalizeCostModelGroupCode(item.Code);
    }
}
