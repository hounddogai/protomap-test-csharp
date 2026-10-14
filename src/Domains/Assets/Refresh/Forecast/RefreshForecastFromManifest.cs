namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Functions of refresh forecast from manifest.</summary>
public static class RefreshForecastFromManifest
{
    public static Dictionary<string, double> SummarizeAssetReturnManifestsForForecast(List<AssetReturnManifest> items)
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

    public static List<RefreshForecast> DeriveRefreshForecastsFromManifest(List<AssetReturnManifest> items)
    {
        var outValue = new List<RefreshForecast>();
        foreach (var item in items)
        {
            var derived = RefreshForecast.NewRefreshForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRefreshForecast(RefreshForecast item)
    {
        return AssetReturnCheckpointPolicy.AssetReturnCheckpointBucket(item.NetExposure());
    }
}
