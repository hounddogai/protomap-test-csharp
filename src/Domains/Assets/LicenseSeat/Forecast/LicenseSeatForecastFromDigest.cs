namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Functions of license seat forecast from digest.</summary>
public static class LicenseSeatForecastFromDigest
{
    public static Dictionary<string, double> SummarizeAssetReturnDigestsForForecast(List<AssetReturnDigest> items)
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

    public static List<LicenseSeatForecast> DeriveLicenseSeatForecastsFromDigest(List<AssetReturnDigest> items)
    {
        var outValue = new List<LicenseSeatForecast>();
        foreach (var item in items)
        {
            var derived = LicenseSeatForecast.NewLicenseSeatForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLicenseSeatForecast(LicenseSeatForecast item)
    {
        return AssetReturnAgreementPolicy.NormalizeAssetReturnAgreementCode(item.Code);
    }
}
