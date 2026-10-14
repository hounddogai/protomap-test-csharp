namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Functions of strict assignment forecast from manifest.</summary>
public static class StrictAssignmentForecastFromManifest
{
    public static Dictionary<string, double> SummarizeCappedAssetReturnManifestsForForecast(List<CappedAssetReturnManifest> items)
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

    public static List<StrictAssignmentForecast> DeriveStrictAssignmentForecastsFromManifest(List<CappedAssetReturnManifest> items)
    {
        var outValue = new List<StrictAssignmentForecast>();
        foreach (var item in items)
        {
            var derived = StrictAssignmentForecast.NewStrictAssignmentForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileStrictAssignmentForecast(StrictAssignmentForecast item)
    {
        return CappedAssetReturnManifestPolicy.CappedAssetReturnManifestBucket(item.AdjustedVolume());
    }
}
