namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Functions of tiered warranty profile from metric.</summary>
public static class TieredWarrantyProfileFromMetric
{
    public static Dictionary<string, double> SummarizeLaptopMetricsForProfile(List<LaptopMetric> items)
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

    public static List<TieredWarrantyProfile> DeriveTieredWarrantyProfilesFromMetric(List<LaptopMetric> items)
    {
        var outValue = new List<TieredWarrantyProfile>();
        foreach (var item in items)
        {
            var derived = TieredWarrantyProfile.NewTieredWarrantyProfile(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredWarrantyProfile(TieredWarrantyProfile item)
    {
        return PrimaryLaptopSlotPolicy.ClassifyPrimaryLaptopSlot(item.NetExposure());
    }
}
