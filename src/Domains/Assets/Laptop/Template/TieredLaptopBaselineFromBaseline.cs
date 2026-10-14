namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Functions of tiered laptop baseline from baseline.</summary>
public static class TieredLaptopBaselineFromBaseline
{
    public static Dictionary<string, double> SummarizeCappedWarrantyBaselinesForBaseline(List<CappedWarrantyBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<TieredLaptopBaseline> DeriveTieredLaptopBaselinesFromBaseline(List<CappedWarrantyBaseline> items)
    {
        var outValue = new List<TieredLaptopBaseline>();
        foreach (var item in items)
        {
            var derived = TieredLaptopBaseline.NewTieredLaptopBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTieredLaptopBaseline(TieredLaptopBaseline item)
    {
        return CappedWarrantyBaselinePolicy.CappedWarrantyBaselineBucket(item.WeightedExposure());
    }
}
