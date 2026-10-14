namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Functions of asset return preference from window.</summary>
public static class AssetReturnPreferenceFromWindow
{
    public static Dictionary<string, double> SummarizeLaptopWindowsForPreference(List<LaptopWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssetReturnPreference> DeriveAssetReturnPreferencesFromWindow(List<LaptopWindow> items)
    {
        var outValue = new List<AssetReturnPreference>();
        foreach (var item in items)
        {
            var derived = AssetReturnPreference.NewAssetReturnPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceAssetReturnPreference(AssetReturnPreference item)
    {
        return LaptopTierPolicy.LaptopTierBucket(item.NetTotal());
    }
}
