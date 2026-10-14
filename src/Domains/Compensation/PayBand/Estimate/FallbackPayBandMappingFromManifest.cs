namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Manifest;

/// <summary>Functions of fallback pay band mapping from manifest.</summary>
public static class FallbackPayBandMappingFromManifest
{
    public static Dictionary<string, double> SummarizePrimaryPayBandManifestsForMapping(List<PrimaryPayBandManifest> items)
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

    public static List<FallbackPayBandMapping> DeriveFallbackPayBandMappingsFromManifest(List<PrimaryPayBandManifest> items)
    {
        var outValue = new List<FallbackPayBandMapping>();
        foreach (var item in items)
        {
            var derived = FallbackPayBandMapping.NewFallbackPayBandMapping(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeFallbackPayBandMapping(FallbackPayBandMapping item)
    {
        return PayBandDigestPolicy.ClassifyPayBandDigest(item.MeasuredValue());
    }
}
