namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Functions of scheduled leave request rollup from digest.</summary>
public static class ScheduledLeaveRequestRollupFromDigest
{
    public static Dictionary<string, double> SummarizeManualAccrualDigestsForRollup(List<ManualAccrualDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ScheduledLeaveRequestRollup> DeriveScheduledLeaveRequestRollupsFromDigest(List<ManualAccrualDigest> items)
    {
        var outValue = new List<ScheduledLeaveRequestRollup>();
        foreach (var item in items)
        {
            var derived = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateScheduledLeaveRequestRollup(ScheduledLeaveRequestRollup item)
    {
        return AccrualSchedulePolicy.NormalizeAccrualScheduleCode(item.Code);
    }
}
