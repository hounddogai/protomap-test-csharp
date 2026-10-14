namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Functions of legacy license seat cycle from metric.</summary>
public static class LegacyLicenseSeatCycleFromMetric
{
    public static Dictionary<string, double> SummarizeLenientLaptopMetricsForCycle(List<LenientLaptopMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<LegacyLicenseSeatCycle> DeriveLegacyLicenseSeatCyclesFromMetric(List<LenientLaptopMetric> items)
    {
        var outValue = new List<LegacyLicenseSeatCycle>();
        foreach (var item in items)
        {
            var derived = LegacyLicenseSeatCycle.NewLegacyLicenseSeatCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLegacyLicenseSeatCycle(LegacyLicenseSeatCycle item)
    {
        return LaptopGroupPolicy.NormalizeLaptopGroupCode(item.Code);
    }
}
