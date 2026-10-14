namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>Functions of primary feedback notice from threshold.</summary>
public static class PrimaryFeedbackNoticeFromThreshold
{
    public static Dictionary<string, double> SummarizeStrictActionPlanThresholdsForNotice(List<StrictActionPlanThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PrimaryFeedbackNotice> DerivePrimaryFeedbackNoticesFromThreshold(List<StrictActionPlanThreshold> items)
    {
        var outValue = new List<PrimaryFeedbackNotice>();
        foreach (var item in items)
        {
            var derived = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPrimaryFeedbackNotice(PrimaryFeedbackNotice item)
    {
        return ActionPlanSnapshotPolicy.NormalizeActionPlanSnapshotCode(item.Code);
    }
}
