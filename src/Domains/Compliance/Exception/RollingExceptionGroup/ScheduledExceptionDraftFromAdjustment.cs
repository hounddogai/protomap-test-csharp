namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Functions of scheduled exception draft from adjustment.</summary>
public static class ScheduledExceptionDraftFromAdjustment
{
    public static Dictionary<string, double> SummarizeAuditAdjustmentsForDraft(List<AuditAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ScheduledExceptionDraft> DeriveScheduledExceptionDraftsFromAdjustment(List<AuditAdjustment> items)
    {
        var outValue = new List<ScheduledExceptionDraft>();
        foreach (var item in items)
        {
            var derived = ScheduledExceptionDraft.NewScheduledExceptionDraft(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeScheduledExceptionDraft(ScheduledExceptionDraft item)
    {
        return PrimaryAuditSchedulePolicy.PrimaryAuditScheduleWeight(9, true);
    }
}
