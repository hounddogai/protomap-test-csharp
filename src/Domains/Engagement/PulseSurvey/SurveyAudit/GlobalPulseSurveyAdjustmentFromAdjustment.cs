namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Functions of global pulse survey adjustment from adjustment.</summary>
public static class GlobalPulseSurveyAdjustmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeScheduledPulseSurveyAdjustmentsForAdjustment(List<ScheduledPulseSurveyAdjustment> items)
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

    public static List<GlobalPulseSurveyAdjustment> DeriveGlobalPulseSurveyAdjustmentsFromAdjustment(List<ScheduledPulseSurveyAdjustment> items)
    {
        var outValue = new List<GlobalPulseSurveyAdjustment>();
        foreach (var item in items)
        {
            var derived = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeGlobalPulseSurveyAdjustment(GlobalPulseSurveyAdjustment item)
    {
        return PendingPulseSurveyAuditPolicy.NormalizePendingPulseSurveyAuditCode(item.Code);
    }
}
