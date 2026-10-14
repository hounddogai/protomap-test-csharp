namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Functions of legacy pay band schedule from tier.</summary>
public static class LegacyPayBandScheduleFromTier
{
    public static Dictionary<string, double> SummarizeBenchmarkTiersForSchedule(List<BenchmarkTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<LegacyPayBandSchedule> DeriveLegacyPayBandSchedulesFromTier(List<BenchmarkTier> items)
    {
        var outValue = new List<LegacyPayBandSchedule>();
        foreach (var item in items)
        {
            var derived = LegacyPayBandSchedule.NewLegacyPayBandSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLegacyPayBandSchedule(LegacyPayBandSchedule item)
    {
        return BenchmarkTierPolicy.ClassifyBenchmarkTier(item.MeasuredValue());
    }
}
