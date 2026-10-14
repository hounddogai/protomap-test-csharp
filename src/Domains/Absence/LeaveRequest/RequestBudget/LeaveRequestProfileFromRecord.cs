namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Functions of leave request profile from record.</summary>
public static class LeaveRequestProfileFromRecord
{
    public static Dictionary<string, double> SummarizePolicyRecordsForProfile(List<PolicyRecord> items)
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

    public static List<LeaveRequestProfile> DeriveLeaveRequestProfilesFromRecord(List<PolicyRecord> items)
    {
        var outValue = new List<LeaveRequestProfile>();
        foreach (var item in items)
        {
            var derived = LeaveRequestProfile.NewLeaveRequestProfile(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLeaveRequestProfile(LeaveRequestProfile item)
    {
        return PolicyBatchPolicy.PolicyBatchWeight(6, true);
    }
}
