namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Functions of manual benchmark window from quota.</summary>
public static class ManualBenchmarkWindowFromQuota
{
    public static Dictionary<string, double> SummarizeCappedPayBandQuotasForWindow(List<CappedPayBandQuota> items)
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

    public static List<ManualBenchmarkWindow> DeriveManualBenchmarkWindowsFromQuota(List<CappedPayBandQuota> items)
    {
        var outValue = new List<ManualBenchmarkWindow>();
        foreach (var item in items)
        {
            var derived = ManualBenchmarkWindow.NewManualBenchmarkWindow(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileManualBenchmarkWindow(ManualBenchmarkWindow item)
    {
        return PayBandCheckpointPolicy.PayBandCheckpointBucket(item.MeasuredValue());
    }
}
