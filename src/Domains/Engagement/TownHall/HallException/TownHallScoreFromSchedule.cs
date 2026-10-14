namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of town hall score from schedule.</summary>
public static class TownHallScoreFromSchedule
{
    public static Dictionary<string, double> SummarizeTownHallSchedulesForScore(List<TownHallSchedule> items)
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

    public static List<TownHallScore> DeriveTownHallScoresFromSchedule(List<TownHallSchedule> items)
    {
        var outValue = new List<TownHallScore>();
        foreach (var item in items)
        {
            var derived = TownHallScore.NewTownHallScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankTownHallScore(TownHallScore item)
    {
        return TownHallMilestonePolicy.TownHallMilestoneBucket(item.MeasuredValue());
    }
}
