namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Functions of global benchmark estimate from profile.</summary>
public static class GlobalBenchmarkEstimateFromProfile
{
    public static Dictionary<string, double> SummarizeGlobalBenchmarkProfilesForEstimate(List<GlobalBenchmarkProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<GlobalBenchmarkEstimate> DeriveGlobalBenchmarkEstimatesFromProfile(List<GlobalBenchmarkProfile> items)
    {
        var outValue = new List<GlobalBenchmarkEstimate>();
        foreach (var item in items)
        {
            var derived = GlobalBenchmarkEstimate.NewGlobalBenchmarkEstimate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeGlobalBenchmarkEstimate(GlobalBenchmarkEstimate item)
    {
        return GlobalBenchmarkProfilePolicy.GlobalBenchmarkProfileWeight(6, true);
    }
}
