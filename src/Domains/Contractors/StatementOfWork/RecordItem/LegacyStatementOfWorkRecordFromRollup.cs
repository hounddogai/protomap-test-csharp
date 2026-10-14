namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Functions of legacy statement of work record from rollup.</summary>
public static class LegacyStatementOfWorkRecordFromRollup
{
    public static Dictionary<string, double> SummarizeSeasonalEngagementRollupsForRecord(List<SeasonalEngagementRollup> items)
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

    public static List<LegacyStatementOfWorkRecord> DeriveLegacyStatementOfWorkRecordsFromRollup(List<SeasonalEngagementRollup> items)
    {
        var outValue = new List<LegacyStatementOfWorkRecord>();
        foreach (var item in items)
        {
            var derived = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacyStatementOfWorkRecordExtra(LegacyStatementOfWorkRecord item)
    {
        return AdaptiveEngagementNoticePolicy.ClassifyAdaptiveEngagementNotice(item.ProjectedLoad());
    }
}
