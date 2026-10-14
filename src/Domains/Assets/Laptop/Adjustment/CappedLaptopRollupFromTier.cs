namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Functions of capped laptop rollup from tier.</summary>
public static class CappedLaptopRollupFromTier
{
    public static Dictionary<string, double> SummarizeAssetReturnTiersForRollup(List<AssetReturnTier> items)
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

    public static List<CappedLaptopRollup> DeriveCappedLaptopRollupsFromTier(List<AssetReturnTier> items)
    {
        var outValue = new List<CappedLaptopRollup>();
        foreach (var item in items)
        {
            var derived = CappedLaptopRollup.NewCappedLaptopRollup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleCappedLaptopRollup(CappedLaptopRollup item)
    {
        return AssetReturnBudgetPolicy.NormalizeAssetReturnBudgetCode(item.Code);
    }
}
