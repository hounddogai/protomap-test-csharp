namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Functions of capped visitor assignment from review.</summary>
public static class CappedVisitorAssignmentFromReview
{
    public static Dictionary<string, double> SummarizeFloorPlanReviewsForAssignment(List<FloorPlanReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<CappedVisitorAssignment> DeriveCappedVisitorAssignmentsFromReview(List<FloorPlanReview> items)
    {
        var outValue = new List<CappedVisitorAssignment>();
        foreach (var item in items)
        {
            var derived = CappedVisitorAssignment.NewCappedVisitorAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyCappedVisitorAssignmentExtra(CappedVisitorAssignment item)
    {
        return FloorPlanExceptionPolicy.FloorPlanExceptionWeight(6, true);
    }
}
