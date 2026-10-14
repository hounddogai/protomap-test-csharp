namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Functions of accrual group from entry.</summary>
public static class AccrualGroupFromEntry
{
    public static Dictionary<string, double> SummarizeLeaveRequestEntriesForGroup(List<LeaveRequestEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<AccrualGroup> DeriveAccrualGroupsFromEntry(List<LeaveRequestEntry> items)
    {
        var outValue = new List<AccrualGroup>();
        foreach (var item in items)
        {
            var derived = AccrualGroup.NewAccrualGroup(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAccrualGroup(AccrualGroup item)
    {
        return LeaveRequestEntryPolicy.LeaveRequestEntryBucket(item.ProjectedValue());
    }
}
