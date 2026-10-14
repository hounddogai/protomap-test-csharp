namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Functions of global town hall checkpoint from adjustment.</summary>
public static class GlobalTownHallCheckpointFromAdjustment
{
    public static Dictionary<string, double> SummarizeScheduledPulseSurveyAdjustmentsForCheckpoint(List<ScheduledPulseSurveyAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<GlobalTownHallCheckpoint> DeriveGlobalTownHallCheckpointsFromAdjustment(List<ScheduledPulseSurveyAdjustment> items)
    {
        var outValue = new List<GlobalTownHallCheckpoint>();
        foreach (var item in items)
        {
            var derived = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankGlobalTownHallCheckpoint(GlobalTownHallCheckpoint item)
    {
        return PendingPulseSurveyAuditPolicy.PendingPulseSurveyAuditWeight(4, true);
    }
}
