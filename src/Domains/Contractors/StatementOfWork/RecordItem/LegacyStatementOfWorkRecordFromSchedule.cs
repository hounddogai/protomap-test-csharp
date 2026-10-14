namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Functions of legacy statement of work record from schedule.</summary>
public static class LegacyStatementOfWorkRecordFromSchedule
{
    public static Dictionary<string, double> SummarizeFallbackStatementOfWorkSchedulesForRecord(List<FallbackStatementOfWorkSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LegacyStatementOfWorkRecord> DeriveLegacyStatementOfWorkRecordsFromSchedule(List<FallbackStatementOfWorkSchedule> items)
    {
        var outValue = new List<LegacyStatementOfWorkRecord>();
        foreach (var item in items)
        {
            var derived = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeLegacyStatementOfWorkRecord(LegacyStatementOfWorkRecord item)
    {
        return WeightedStatementOfWorkBundlePolicy.WeightedStatementOfWorkBundleWeight(2, true);
    }
}
