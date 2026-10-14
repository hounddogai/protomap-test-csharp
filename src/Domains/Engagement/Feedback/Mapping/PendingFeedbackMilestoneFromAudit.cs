namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Functions of pending feedback milestone from audit.</summary>
public static class PendingFeedbackMilestoneFromAudit
{
    public static Dictionary<string, double> SummarizeCappedPulseSurveyAuditsForMilestone(List<CappedPulseSurveyAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PendingFeedbackMilestone> DerivePendingFeedbackMilestonesFromAudit(List<CappedPulseSurveyAudit> items)
    {
        var outValue = new List<PendingFeedbackMilestone>();
        foreach (var item in items)
        {
            var derived = PendingFeedbackMilestone.NewPendingFeedbackMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePendingFeedbackMilestone(PendingFeedbackMilestone item)
    {
        return CappedPulseSurveyAuditPolicy.NormalizeCappedPulseSurveyAuditCode(item.Code);
    }
}
