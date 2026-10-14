namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Functions of span estimate from summary.</summary>
public static class SpanEstimateFromSummary
{
    public static Dictionary<string, double> SummarizeManualPositionSummariesForEstimate(List<ManualPositionSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<SpanEstimate> DeriveSpanEstimatesFromSummary(List<ManualPositionSummary> items)
    {
        var outValue = new List<SpanEstimate>();
        foreach (var item in items)
        {
            var derived = SpanEstimate.NewSpanEstimate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceSpanEstimate(SpanEstimate item)
    {
        return PendingPositionReviewPolicy.ClassifyPendingPositionReview(item.ProjectedExposure());
    }
}
