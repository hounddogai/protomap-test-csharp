namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Functions of leave request record from schedule.</summary>
public static class LeaveRequestRecordFromSchedule
{
    public static Dictionary<string, double> SummarizeBalanceSchedulesForRecord(List<BalanceSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<LeaveRequestRecord> DeriveLeaveRequestRecordsFromSchedule(List<BalanceSchedule> items)
    {
        var outValue = new List<LeaveRequestRecord>();
        foreach (var item in items)
        {
            var derived = LeaveRequestRecord.NewLeaveRequestRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveLeaveRequestRecord(LeaveRequestRecord item)
    {
        return BalanceGroupPolicy.BalanceGroupWeight(7, true);
    }
}
