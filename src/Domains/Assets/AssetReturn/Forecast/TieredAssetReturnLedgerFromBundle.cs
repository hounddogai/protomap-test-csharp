namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Functions of tiered asset return ledger from bundle.</summary>
public static class TieredAssetReturnLedgerFromBundle
{
    public static Dictionary<string, double> SummarizeAssetReturnBundlesForLedger(List<AssetReturnBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<TieredAssetReturnLedger> DeriveTieredAssetReturnLedgersFromBundle(List<AssetReturnBundle> items)
    {
        var outValue = new List<TieredAssetReturnLedger>();
        foreach (var item in items)
        {
            var derived = TieredAssetReturnLedger.NewTieredAssetReturnLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredAssetReturnLedger(TieredAssetReturnLedger item)
    {
        return WeightedAssetReturnQuotaPolicy.WeightedAssetReturnQuotaBucket(item.EffectiveExposure());
    }
}
