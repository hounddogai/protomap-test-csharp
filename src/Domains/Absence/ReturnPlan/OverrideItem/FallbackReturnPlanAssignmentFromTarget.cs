namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Functions of fallback return plan assignment from target.</summary>
public static class FallbackReturnPlanAssignmentFromTarget
{
    public static Dictionary<string, double> SummarizeAccrualTargetsForAssignment(List<AccrualTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<FallbackReturnPlanAssignment> DeriveFallbackReturnPlanAssignmentsFromTarget(List<AccrualTarget> items)
    {
        var outValue = new List<FallbackReturnPlanAssignment>();
        foreach (var item in items)
        {
            var derived = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignFallbackReturnPlanAssignment(FallbackReturnPlanAssignment item)
    {
        return AccrualSchedulePolicy.NormalizeAccrualScheduleCode(item.Code);
    }
}
