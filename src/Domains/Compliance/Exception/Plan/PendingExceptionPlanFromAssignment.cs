namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Functions of pending exception plan from assignment.</summary>
public static class PendingExceptionPlanFromAssignment
{
    public static Dictionary<string, double> SummarizeAttestationAssignmentsForPlan(List<AttestationAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<PendingExceptionPlan> DerivePendingExceptionPlansFromAssignment(List<AttestationAssignment> items)
    {
        var outValue = new List<PendingExceptionPlan>();
        foreach (var item in items)
        {
            var derived = PendingExceptionPlan.NewPendingExceptionPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcilePendingExceptionPlan(PendingExceptionPlan item)
    {
        return AttestationBaselinePolicy.AttestationBaselineWeight(7, true);
    }
}
