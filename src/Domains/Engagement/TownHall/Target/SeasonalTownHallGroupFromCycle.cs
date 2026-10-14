namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Functions of seasonal town hall group from cycle.</summary>
public static class SeasonalTownHallGroupFromCycle
{
    public static Dictionary<string, double> SummarizeSeasonalTownHallCyclesForGroup(List<SeasonalTownHallCycle> items)
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

    public static List<SeasonalTownHallGroup> DeriveSeasonalTownHallGroupsFromCycle(List<SeasonalTownHallCycle> items)
    {
        var outValue = new List<SeasonalTownHallGroup>();
        foreach (var item in items)
        {
            var derived = SeasonalTownHallGroup.NewSeasonalTownHallGroup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSeasonalTownHallGroup(SeasonalTownHallGroup item)
    {
        return TownHallEstimatePolicy.NormalizeTownHallEstimateCode(item.Code);
    }
}
