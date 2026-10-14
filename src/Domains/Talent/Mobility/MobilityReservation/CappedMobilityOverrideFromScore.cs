namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Functions of capped mobility override from score.</summary>
public static class CappedMobilityOverrideFromScore
{
    public static Dictionary<string, double> SummarizeManualNominationScoresForOverride(List<ManualNominationScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CappedMobilityOverride> DeriveCappedMobilityOverridesFromScore(List<ManualNominationScore> items)
    {
        var outValue = new List<CappedMobilityOverride>();
        foreach (var item in items)
        {
            var derived = CappedMobilityOverride.NewCappedMobilityOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateCappedMobilityOverride(CappedMobilityOverride item)
    {
        return FallbackNominationBundlePolicy.FallbackNominationBundleBucket(item.EffectiveLoad());
    }
}
