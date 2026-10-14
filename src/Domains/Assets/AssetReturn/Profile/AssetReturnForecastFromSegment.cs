namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Functions of asset return forecast from segment.</summary>
public static class AssetReturnForecastFromSegment
{
    public static Dictionary<string, double> SummarizeLenientLicenseSeatSegmentsForForecast(List<LenientLicenseSeatSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<AssetReturnForecast> DeriveAssetReturnForecastsFromSegment(List<LenientLicenseSeatSegment> items)
    {
        var outValue = new List<AssetReturnForecast>();
        foreach (var item in items)
        {
            var derived = AssetReturnForecast.NewAssetReturnForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAssetReturnForecast(AssetReturnForecast item)
    {
        return LicenseSeatLedgerPolicy.LicenseSeatLedgerBucket(item.EffectiveLoad());
    }
}
