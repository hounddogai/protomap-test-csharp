namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Functions of fallback asset return forecast from policy.</summary>
public static class FallbackAssetReturnForecastFromPolicy
{
    public static Dictionary<string, double> SummarizeAssignmentPoliciesForForecast(List<AssignmentPolicy> items)
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

    public static List<FallbackAssetReturnForecast> DeriveFallbackAssetReturnForecastsFromPolicy(List<AssignmentPolicy> items)
    {
        var outValue = new List<FallbackAssetReturnForecast>();
        foreach (var item in items)
        {
            var derived = FallbackAssetReturnForecast.NewFallbackAssetReturnForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFallbackAssetReturnForecast(FallbackAssetReturnForecast item)
    {
        return RegionalAssignmentAgreementPolicy.ClassifyRegionalAssignmentAgreement(item.AdjustedVolume());
    }
}
