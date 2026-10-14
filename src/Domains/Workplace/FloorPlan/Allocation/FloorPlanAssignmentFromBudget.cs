namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Functions of floor plan assignment from budget.</summary>
public static class FloorPlanAssignmentFromBudget
{
    public static Dictionary<string, double> SummarizeStrictDeskBudgetsForAssignment(List<StrictDeskBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<FloorPlanAssignment> DeriveFloorPlanAssignmentsFromBudget(List<StrictDeskBudget> items)
    {
        var outValue = new List<FloorPlanAssignment>();
        foreach (var item in items)
        {
            var derived = FloorPlanAssignment.NewFloorPlanAssignment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyFloorPlanAssignmentExtra(FloorPlanAssignment item)
    {
        return StrictDeskBudgetPolicy.NormalizeStrictDeskBudgetCode(item.Code);
    }
}
