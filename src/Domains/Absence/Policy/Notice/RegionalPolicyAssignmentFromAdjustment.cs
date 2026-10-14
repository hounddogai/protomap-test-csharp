namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Functions of regional policy assignment from adjustment.</summary>
public static class RegionalPolicyAssignmentFromAdjustment
{
    public static Dictionary<string, double> SummarizePendingPolicyAdjustmentsForAssignment(List<PendingPolicyAdjustment> items)
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

    public static List<RegionalPolicyAssignment> DeriveRegionalPolicyAssignmentsFromAdjustment(List<PendingPolicyAdjustment> items)
    {
        var outValue = new List<RegionalPolicyAssignment>();
        foreach (var item in items)
        {
            var derived = RegionalPolicyAssignment.NewRegionalPolicyAssignment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRegionalPolicyAssignment(RegionalPolicyAssignment item)
    {
        return AdaptivePolicyQuotaPolicy.NormalizeAdaptivePolicyQuotaCode(item.Code);
    }
}
