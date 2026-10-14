namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Functions of rolling leave request adjustment from policy.</summary>
public static class RollingLeaveRequestAdjustmentFromPolicy
{
    public static Dictionary<string, double> SummarizeLeaveRequestPoliciesForAdjustment(List<LeaveRequestPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<RollingLeaveRequestAdjustment> DeriveRollingLeaveRequestAdjustmentsFromPolicy(List<LeaveRequestPolicy> items)
    {
        var outValue = new List<RollingLeaveRequestAdjustment>();
        foreach (var item in items)
        {
            var derived = RollingLeaveRequestAdjustment.NewRollingLeaveRequestAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveRollingLeaveRequestAdjustment(RollingLeaveRequestAdjustment item)
    {
        return ScheduledLeaveRequestMappingPolicy.ScheduledLeaveRequestMappingWeight(3, true);
    }
}
