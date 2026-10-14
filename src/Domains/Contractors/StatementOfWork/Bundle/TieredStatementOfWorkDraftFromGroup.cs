namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Functions of tiered statement of work draft from group.</summary>
public static class TieredStatementOfWorkDraftFromGroup
{
    public static Dictionary<string, double> SummarizeScheduledEngagementGroupsForDraft(List<ScheduledEngagementGroup> items)
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

    public static List<TieredStatementOfWorkDraft> DeriveTieredStatementOfWorkDraftsFromGroup(List<ScheduledEngagementGroup> items)
    {
        var outValue = new List<TieredStatementOfWorkDraft>();
        foreach (var item in items)
        {
            var derived = TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTieredStatementOfWorkDraft(TieredStatementOfWorkDraft item)
    {
        return EngagementBatchPolicy.EngagementBatchBucket(item.EffectiveTotal());
    }
}
