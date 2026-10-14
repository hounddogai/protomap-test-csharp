namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Functions of return plan assignment from manifest.</summary>
public static class ReturnPlanAssignmentFromManifest
{
    public static Dictionary<string, double> SummarizeReturnPlanManifestsForAssignment(List<ReturnPlanManifest> items)
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

    public static List<ReturnPlanAssignment> DeriveReturnPlanAssignmentsFromManifest(List<ReturnPlanManifest> items)
    {
        var outValue = new List<ReturnPlanAssignment>();
        foreach (var item in items)
        {
            var derived = ReturnPlanAssignment.NewReturnPlanAssignment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanReturnPlanAssignment(ReturnPlanAssignment item)
    {
        return ReturnPlanManifestPolicy.ReturnPlanManifestBucket(item.MeasuredValue());
    }
}
