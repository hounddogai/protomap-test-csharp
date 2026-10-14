namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Functions of access badge assignment from rollup.</summary>
public static class AccessBadgeAssignmentFromRollup
{
    public static Dictionary<string, double> SummarizeVisitorRollupsForAssignment(List<VisitorRollup> items)
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

    public static List<AccessBadgeAssignment> DeriveAccessBadgeAssignmentsFromRollup(List<VisitorRollup> items)
    {
        var outValue = new List<AccessBadgeAssignment>();
        foreach (var item in items)
        {
            var derived = AccessBadgeAssignment.NewAccessBadgeAssignment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAccessBadgeAssignment(AccessBadgeAssignment item)
    {
        return WeightedVisitorBaselinePolicy.WeightedVisitorBaselineBucket(item.NetExposure());
    }
}
