namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Functions of weighted merit cycle bundle from segment.</summary>
public static class WeightedMeritCycleBundleFromSegment
{
    public static Dictionary<string, double> SummarizeAdaptiveBenchmarkSegmentsForBundle(List<AdaptiveBenchmarkSegment> items)
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

    public static List<WeightedMeritCycleBundle> DeriveWeightedMeritCycleBundlesFromSegment(List<AdaptiveBenchmarkSegment> items)
    {
        var outValue = new List<WeightedMeritCycleBundle>();
        foreach (var item in items)
        {
            var derived = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeWeightedMeritCycleBundle(WeightedMeritCycleBundle item)
    {
        return ScheduledBenchmarkSchedulePolicy.NormalizeScheduledBenchmarkScheduleCode(item.Code);
    }
}
