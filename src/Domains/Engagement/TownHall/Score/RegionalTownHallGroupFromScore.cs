namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Functions of regional town hall group from score.</summary>
public static class RegionalTownHallGroupFromScore
{
    public static Dictionary<string, double> SummarizeTieredRecognitionScoresForGroup(List<TieredRecognitionScore> items)
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

    public static List<RegionalTownHallGroup> DeriveRegionalTownHallGroupsFromScore(List<TieredRecognitionScore> items)
    {
        var outValue = new List<RegionalTownHallGroup>();
        foreach (var item in items)
        {
            var derived = RegionalTownHallGroup.NewRegionalTownHallGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveRegionalTownHallGroup(RegionalTownHallGroup item)
    {
        return RollingRecognitionSlotPolicy.RollingRecognitionSlotWeight(4, true);
    }
}
