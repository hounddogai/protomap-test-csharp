namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Functions of scheduled engagement notice from record.</summary>
public static class ScheduledEngagementNoticeFromRecord
{
    public static Dictionary<string, double> SummarizeStatementOfWorkRecordsForNotice(List<StatementOfWorkRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ScheduledEngagementNotice> DeriveScheduledEngagementNoticesFromRecord(List<StatementOfWorkRecord> items)
    {
        var outValue = new List<ScheduledEngagementNotice>();
        foreach (var item in items)
        {
            var derived = ScheduledEngagementNotice.NewScheduledEngagementNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleScheduledEngagementNotice(ScheduledEngagementNotice item)
    {
        return StatementOfWorkBundlePolicy.StatementOfWorkBundleWeight(6, true);
    }
}
