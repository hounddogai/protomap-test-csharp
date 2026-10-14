namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Functions of policy allocation from draft.</summary>
public static class PolicyAllocationFromDraft
{
    public static Dictionary<string, double> SummarizeLeaveRequestDraftsForAllocation(List<LeaveRequestDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PolicyAllocation> DerivePolicyAllocationsFromDraft(List<LeaveRequestDraft> items)
    {
        var outValue = new List<PolicyAllocation>();
        foreach (var item in items)
        {
            var derived = PolicyAllocation.NewPolicyAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePolicyAllocation(PolicyAllocation item)
    {
        return ScheduledLeaveRequestSnapshotPolicy.NormalizeScheduledLeaveRequestSnapshotCode(item.Code);
    }
}
