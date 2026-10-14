namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Functions of seasonal town hall cycle from snapshot.</summary>
public static class SeasonalTownHallCycleFromSnapshot
{
    public static Dictionary<string, double> SummarizePrimaryPulseSurveySnapshotsForCycle(List<PrimaryPulseSurveySnapshot> items)
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

    public static List<SeasonalTownHallCycle> DeriveSeasonalTownHallCyclesFromSnapshot(List<PrimaryPulseSurveySnapshot> items)
    {
        var outValue = new List<SeasonalTownHallCycle>();
        foreach (var item in items)
        {
            var derived = SeasonalTownHallCycle.NewSeasonalTownHallCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSeasonalTownHallCycle(SeasonalTownHallCycle item)
    {
        return PrimaryPulseSurveySnapshotPolicy.PrimaryPulseSurveySnapshotWeight(1, true);
    }
}
