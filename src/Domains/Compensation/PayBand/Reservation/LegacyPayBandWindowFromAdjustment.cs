namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

/// <summary>Functions of legacy pay band window from adjustment.</summary>
public static class LegacyPayBandWindowFromAdjustment
{
    public static Dictionary<string, double> SummarizeRollingBenchmarkAdjustmentsForWindow(List<RollingBenchmarkAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LegacyPayBandWindow> DeriveLegacyPayBandWindowsFromAdjustment(List<RollingBenchmarkAdjustment> items)
    {
        var outValue = new List<LegacyPayBandWindow>();
        foreach (var item in items)
        {
            var derived = LegacyPayBandWindow.NewLegacyPayBandWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchLegacyPayBandWindow(LegacyPayBandWindow item)
    {
        return SeasonalBenchmarkGroupPolicy.SeasonalBenchmarkGroupWeight(5, true);
    }
}
