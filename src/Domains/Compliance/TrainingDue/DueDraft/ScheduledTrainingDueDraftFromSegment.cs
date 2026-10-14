namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Functions of scheduled training due draft from segment.</summary>
public static class ScheduledTrainingDueDraftFromSegment
{
    public static Dictionary<string, double> SummarizeAuditSegmentsForDraft(List<AuditSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ScheduledTrainingDueDraft> DeriveScheduledTrainingDueDraftsFromSegment(List<AuditSegment> items)
    {
        var outValue = new List<ScheduledTrainingDueDraft>();
        foreach (var item in items)
        {
            var derived = ScheduledTrainingDueDraft.NewScheduledTrainingDueDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreScheduledTrainingDueDraft(ScheduledTrainingDueDraft item)
    {
        return AuditThresholdPolicy.AuditThresholdBucket(item.NetTotal());
    }
}
