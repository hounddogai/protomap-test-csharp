namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Functions of benchmark window from threshold.</summary>
public static class BenchmarkWindowFromThreshold
{
    public static Dictionary<string, double> SummarizeBatchedPayBandThresholdsForWindow(List<BatchedPayBandThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<BenchmarkWindow> DeriveBenchmarkWindowsFromThreshold(List<BatchedPayBandThreshold> items)
    {
        var outValue = new List<BenchmarkWindow>();
        foreach (var item in items)
        {
            var derived = BenchmarkWindow.NewBenchmarkWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileBenchmarkWindow(BenchmarkWindow item)
    {
        return PayBandAdjustmentPolicy.PayBandAdjustmentBucket(item.WeightedExposure());
    }
}
