namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Functions of manual pulse survey audit from threshold.</summary>
public static class ManualPulseSurveyAuditFromThreshold
{
    public static Dictionary<string, double> SummarizeRollingTownHallThresholdsForAudit(List<RollingTownHallThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<ManualPulseSurveyAudit> DeriveManualPulseSurveyAuditsFromThreshold(List<RollingTownHallThreshold> items)
    {
        var outValue = new List<ManualPulseSurveyAudit>();
        foreach (var item in items)
        {
            var derived = ManualPulseSurveyAudit.NewManualPulseSurveyAudit(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanManualPulseSurveyAudit(ManualPulseSurveyAudit item)
    {
        return TownHallAdjustmentPolicy.ClassifyTownHallAdjustment(item.ProjectedLoad());
    }
}
