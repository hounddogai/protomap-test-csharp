namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Functions of fallback policy ack preference from forecast.</summary>
public static class FallbackPolicyAckPreferenceFromForecast
{
    public static Dictionary<string, double> SummarizeManualCertificationForecastsForPreference(List<ManualCertificationForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<FallbackPolicyAckPreference> DeriveFallbackPolicyAckPreferencesFromForecast(List<ManualCertificationForecast> items)
    {
        var outValue = new List<FallbackPolicyAckPreference>();
        foreach (var item in items)
        {
            var derived = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFallbackPolicyAckPreference(FallbackPolicyAckPreference item)
    {
        return ManualCertificationForecastPolicy.ManualCertificationForecastBucket(item.ProjectedValue());
    }
}
