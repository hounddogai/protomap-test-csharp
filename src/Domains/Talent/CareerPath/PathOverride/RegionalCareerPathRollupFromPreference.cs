namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Functions of regional career path rollup from preference.</summary>
public static class RegionalCareerPathRollupFromPreference
{
    public static Dictionary<string, double> SummarizeScheduledMobilityPreferencesForRollup(List<ScheduledMobilityPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RegionalCareerPathRollup> DeriveRegionalCareerPathRollupsFromPreference(List<ScheduledMobilityPreference> items)
    {
        var outValue = new List<RegionalCareerPathRollup>();
        foreach (var item in items)
        {
            var derived = RegionalCareerPathRollup.NewRegionalCareerPathRollup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeRegionalCareerPathRollup(RegionalCareerPathRollup item)
    {
        return ScheduledMobilityPreferencePolicy.ScheduledMobilityPreferenceWeight(6, true);
    }
}
