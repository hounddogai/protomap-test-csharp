namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Functions of legacy leave request record from segment.</summary>
public static class LegacyLeaveRequestRecordFromSegment
{
    public static Dictionary<string, double> SummarizeBalanceSegmentsForRecord(List<BalanceSegment> items)
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

    public static List<LegacyLeaveRequestRecord> DeriveLegacyLeaveRequestRecordsFromSegment(List<BalanceSegment> items)
    {
        var outValue = new List<LegacyLeaveRequestRecord>();
        foreach (var item in items)
        {
            var derived = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLegacyLeaveRequestRecord(LegacyLeaveRequestRecord item)
    {
        return BalanceReviewPolicy.NormalizeBalanceReviewCode(item.Code);
    }
}
