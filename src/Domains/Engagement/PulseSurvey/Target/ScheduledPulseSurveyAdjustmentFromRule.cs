namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Functions of scheduled pulse survey adjustment from rule.</summary>
public static class ScheduledPulseSurveyAdjustmentFromRule
{
    public static Dictionary<string, double> SummarizeAdaptiveTownHallRulesForAdjustment(List<AdaptiveTownHallRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<ScheduledPulseSurveyAdjustment> DeriveScheduledPulseSurveyAdjustmentsFromRule(List<AdaptiveTownHallRule> items)
    {
        var outValue = new List<ScheduledPulseSurveyAdjustment>();
        foreach (var item in items)
        {
            var derived = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceScheduledPulseSurveyAdjustment(ScheduledPulseSurveyAdjustment item)
    {
        return AdaptiveTownHallTierPolicy.ClassifyAdaptiveTownHallTier(item.EffectiveTotal());
    }
}
