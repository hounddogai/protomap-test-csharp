namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Functions of rolling mobility notice from preference.</summary>
public static class RollingMobilityNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeCareerPathPreferencesForNotice(List<CareerPathPreference> items)
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

    public static List<RollingMobilityNotice> DeriveRollingMobilityNoticesFromPreference(List<CareerPathPreference> items)
    {
        var outValue = new List<RollingMobilityNotice>();
        foreach (var item in items)
        {
            var derived = RollingMobilityNotice.NewRollingMobilityNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRollingMobilityNotice(RollingMobilityNotice item)
    {
        return CareerPathPreferencePolicy.ClassifyCareerPathPreference(item.WeightedVolume());
    }
}
