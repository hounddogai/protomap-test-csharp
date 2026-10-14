namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Functions of seasonal policy ack estimate from baseline.</summary>
public static class SeasonalPolicyAckEstimateFromBaseline
{
    public static Dictionary<string, double> SummarizeAuditBaselinesForEstimate(List<AuditBaseline> items)
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

    public static List<SeasonalPolicyAckEstimate> DeriveSeasonalPolicyAckEstimatesFromBaseline(List<AuditBaseline> items)
    {
        var outValue = new List<SeasonalPolicyAckEstimate>();
        foreach (var item in items)
        {
            var derived = SeasonalPolicyAckEstimate.NewSeasonalPolicyAckEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateSeasonalPolicyAckEstimate(SeasonalPolicyAckEstimate item)
    {
        return AuditBaselinePolicy.ClassifyAuditBaseline(item.WeightedTotal());
    }
}
