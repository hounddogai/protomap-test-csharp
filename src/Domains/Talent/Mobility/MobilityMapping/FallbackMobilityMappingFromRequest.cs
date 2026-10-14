namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Functions of fallback mobility mapping from request.</summary>
public static class FallbackMobilityMappingFromRequest
{
    public static Dictionary<string, double> SummarizeRollingSuccessionPlanRequestsForMapping(List<RollingSuccessionPlanRequest> items)
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

    public static List<FallbackMobilityMapping> DeriveFallbackMobilityMappingsFromRequest(List<RollingSuccessionPlanRequest> items)
    {
        var outValue = new List<FallbackMobilityMapping>();
        foreach (var item in items)
        {
            var derived = FallbackMobilityMapping.NewFallbackMobilityMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFallbackMobilityMapping(FallbackMobilityMapping item)
    {
        return FallbackSuccessionPlanQuotaPolicy.FallbackSuccessionPlanQuotaBucket(item.WeightedVolume());
    }
}
