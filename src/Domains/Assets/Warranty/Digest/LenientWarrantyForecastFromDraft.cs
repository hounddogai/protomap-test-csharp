namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Functions of lenient warranty forecast from draft.</summary>
public static class LenientWarrantyForecastFromDraft
{
    public static Dictionary<string, double> SummarizeGlobalLaptopDraftsForForecast(List<GlobalLaptopDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<LenientWarrantyForecast> DeriveLenientWarrantyForecastsFromDraft(List<GlobalLaptopDraft> items)
    {
        var outValue = new List<LenientWarrantyForecast>();
        foreach (var item in items)
        {
            var derived = LenientWarrantyForecast.NewLenientWarrantyForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLenientWarrantyForecast(LenientWarrantyForecast item)
    {
        return TieredLaptopWindowPolicy.TieredLaptopWindowBucket(item.WeightedVolume());
    }
}
