namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Functions of adaptive nomination variance from profile.</summary>
public static class AdaptiveNominationVarianceFromProfile
{
    public static Dictionary<string, double> SummarizeNominationProfilesForVariance(List<NominationProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<AdaptiveNominationVariance> DeriveAdaptiveNominationVariancesFromProfile(List<NominationProfile> items)
    {
        var outValue = new List<AdaptiveNominationVariance>();
        foreach (var item in items)
        {
            var derived = AdaptiveNominationVariance.NewAdaptiveNominationVariance(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateAdaptiveNominationVariance(AdaptiveNominationVariance item)
    {
        return NominationThresholdPolicy.NormalizeNominationThresholdCode(item.Code);
    }
}
