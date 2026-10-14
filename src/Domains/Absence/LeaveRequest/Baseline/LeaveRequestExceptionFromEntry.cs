namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Functions of leave request exception from entry.</summary>
public static class LeaveRequestExceptionFromEntry
{
    public static Dictionary<string, double> SummarizeLeaveRequestEntriesForException(List<LeaveRequestEntry> items)
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

    public static List<LeaveRequestException> DeriveLeaveRequestExceptionsFromEntry(List<LeaveRequestEntry> items)
    {
        var outValue = new List<LeaveRequestException>();
        foreach (var item in items)
        {
            var derived = LeaveRequestException.NewLeaveRequestException(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanLeaveRequestException(LeaveRequestException item)
    {
        return LeaveRequestEntryPolicy.ClassifyLeaveRequestEntry(item.MeasuredValue());
    }
}
