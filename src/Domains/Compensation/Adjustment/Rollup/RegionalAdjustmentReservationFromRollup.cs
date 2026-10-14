namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Functions of regional adjustment reservation from rollup.</summary>
public static class RegionalAdjustmentReservationFromRollup
{
    public static Dictionary<string, double> SummarizeBenchmarkRollupsForReservation(List<BenchmarkRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RegionalAdjustmentReservation> DeriveRegionalAdjustmentReservationsFromRollup(List<BenchmarkRollup> items)
    {
        var outValue = new List<RegionalAdjustmentReservation>();
        foreach (var item in items)
        {
            var derived = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyRegionalAdjustmentReservation(RegionalAdjustmentReservation item)
    {
        return GlobalBenchmarkProfilePolicy.GlobalBenchmarkProfileWeight(3, true);
    }
}
