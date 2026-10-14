namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Functions of lenient benchmark manifest from window.</summary>
public static class LenientBenchmarkManifestFromWindow
{
    public static Dictionary<string, double> SummarizeWeightedEquityGrantWindowsForManifest(List<WeightedEquityGrantWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LenientBenchmarkManifest> DeriveLenientBenchmarkManifestsFromWindow(List<WeightedEquityGrantWindow> items)
    {
        var outValue = new List<LenientBenchmarkManifest>();
        foreach (var item in items)
        {
            var derived = LenientBenchmarkManifest.NewLenientBenchmarkManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientBenchmarkManifest(LenientBenchmarkManifest item)
    {
        return EquityGrantExceptionPolicy.EquityGrantExceptionBucket(item.EffectiveVolume());
    }
}
