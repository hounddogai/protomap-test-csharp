namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Functions of legacy nomination rollup from preference.</summary>
public static class LegacyNominationRollupFromPreference
{
    public static Dictionary<string, double> SummarizeCareerPathPreferencesForRollup(List<CareerPathPreference> items)
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

    public static List<LegacyNominationRollup> DeriveLegacyNominationRollupsFromPreference(List<CareerPathPreference> items)
    {
        var outValue = new List<LegacyNominationRollup>();
        foreach (var item in items)
        {
            var derived = LegacyNominationRollup.NewLegacyNominationRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLegacyNominationRollup(LegacyNominationRollup item)
    {
        return LegacyCareerPathEntryPolicy.LegacyCareerPathEntryBucket(item.AdjustedVolume());
    }
}
