namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Functions of position estimate from quota.</summary>
public static class PositionEstimateFromQuota
{
    public static Dictionary<string, double> SummarizeFallbackPositionQuotasForEstimate(List<FallbackPositionQuota> items)
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

    public static List<PositionEstimate> DerivePositionEstimatesFromQuota(List<FallbackPositionQuota> items)
    {
        var outValue = new List<PositionEstimate>();
        foreach (var item in items)
        {
            var derived = PositionEstimate.NewPositionEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizePositionEstimate(PositionEstimate item)
    {
        return LenientPositionSegmentPolicy.LenientPositionSegmentWeight(9, true);
    }
}
