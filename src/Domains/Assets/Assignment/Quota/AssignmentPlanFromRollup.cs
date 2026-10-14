namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>Functions of assignment plan from rollup.</summary>
public static class AssignmentPlanFromRollup
{
    public static Dictionary<string, double> SummarizeAssignmentRollupsForPlan(List<AssignmentRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<AssignmentPlan> DeriveAssignmentPlansFromRollup(List<AssignmentRollup> items)
    {
        var outValue = new List<AssignmentPlan>();
        foreach (var item in items)
        {
            var derived = AssignmentPlan.NewAssignmentPlan(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAssignmentPlan(AssignmentPlan item)
    {
        return AssignmentDraftPolicy.NormalizeAssignmentDraftCode(item.Code);
    }
}
