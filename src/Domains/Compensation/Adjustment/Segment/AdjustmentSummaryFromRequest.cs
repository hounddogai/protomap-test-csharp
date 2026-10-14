namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Functions of adjustment summary from request.</summary>
public static class AdjustmentSummaryFromRequest
{
    public static Dictionary<string, double> SummarizeAdjustmentRequestsForSummary(List<AdjustmentRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdjustmentSummary> DeriveAdjustmentSummariesFromRequest(List<AdjustmentRequest> items)
    {
        var outValue = new List<AdjustmentSummary>();
        foreach (var item in items)
        {
            var derived = AdjustmentSummary.NewAdjustmentSummary(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAdjustmentSummary(AdjustmentSummary item)
    {
        return AdjustmentTierPolicy.NormalizeAdjustmentTierCode(item.Code);
    }
}
