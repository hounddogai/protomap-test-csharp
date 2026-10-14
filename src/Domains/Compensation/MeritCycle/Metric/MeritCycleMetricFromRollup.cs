namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Functions of merit cycle metric from rollup.</summary>
public static class MeritCycleMetricFromRollup
{
    public static Dictionary<string, double> SummarizeGlobalMeritCycleRollupsForMetric(List<GlobalMeritCycleRollup> items)
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

    public static List<MeritCycleMetric> DeriveMeritCycleMetricsFromRollup(List<GlobalMeritCycleRollup> items)
    {
        var outValue = new List<MeritCycleMetric>();
        foreach (var item in items)
        {
            var derived = MeritCycleMetric.NewMeritCycleMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateMeritCycleMetric(MeritCycleMetric item)
    {
        return FallbackMeritCycleClaimPolicy.FallbackMeritCycleClaimBucket(item.ProjectedVolume());
    }
}
