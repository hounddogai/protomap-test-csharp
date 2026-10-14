namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Functions of capped laptop rollup from manifest.</summary>
public static class CappedLaptopRollupFromManifest
{
    public static Dictionary<string, double> SummarizeTieredWarrantyManifestsForRollup(List<TieredWarrantyManifest> items)
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

    public static List<CappedLaptopRollup> DeriveCappedLaptopRollupsFromManifest(List<TieredWarrantyManifest> items)
    {
        var outValue = new List<CappedLaptopRollup>();
        foreach (var item in items)
        {
            var derived = CappedLaptopRollup.NewCappedLaptopRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreCappedLaptopRollup(CappedLaptopRollup item)
    {
        return WarrantyGroupPolicy.WarrantyGroupWeight(5, true);
    }
}
