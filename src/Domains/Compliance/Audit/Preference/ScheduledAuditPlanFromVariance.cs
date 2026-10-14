namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Compliance.Exception.ExceptionAssignment;

/// <summary>Functions of scheduled audit plan from variance.</summary>
public static class ScheduledAuditPlanFromVariance
{
    public static Dictionary<string, double> SummarizeExceptionVariancesForPlan(List<ExceptionVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ScheduledAuditPlan> DeriveScheduledAuditPlansFromVariance(List<ExceptionVariance> items)
    {
        var outValue = new List<ScheduledAuditPlan>();
        foreach (var item in items)
        {
            var derived = ScheduledAuditPlan.NewScheduledAuditPlan(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceScheduledAuditPlan(ScheduledAuditPlan item)
    {
        return ScheduledExceptionAssignmentPolicy.ClassifyScheduledExceptionAssignment(item.ProjectedValue());
    }
}
