namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Functions of legacy town hall schedule from cycle.</summary>
public static class LegacyTownHallScheduleFromCycle
{
    public static Dictionary<string, double> SummarizeFeedbackCyclesForSchedule(List<FeedbackCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LegacyTownHallSchedule> DeriveLegacyTownHallSchedulesFromCycle(List<FeedbackCycle> items)
    {
        var outValue = new List<LegacyTownHallSchedule>();
        foreach (var item in items)
        {
            var derived = LegacyTownHallSchedule.NewLegacyTownHallSchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLegacyTownHallSchedule(LegacyTownHallSchedule item)
    {
        return FeedbackVariancePolicy.FeedbackVarianceBucket(item.NetLoad());
    }
}
