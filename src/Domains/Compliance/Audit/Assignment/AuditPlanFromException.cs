namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Functions of audit plan from exception.</summary>
public static class AuditPlanFromException
{
    public static Dictionary<string, double> SummarizeRollingTrainingDueExceptionsForPlan(List<RollingTrainingDueException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<AuditPlan> DeriveAuditPlansFromException(List<RollingTrainingDueException> items)
    {
        var outValue = new List<AuditPlan>();
        foreach (var item in items)
        {
            var derived = AuditPlan.NewAuditPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAuditPlan(AuditPlan item)
    {
        return CappedTrainingDueTargetPolicy.ClassifyCappedTrainingDueTarget(item.AdjustedValue());
    }
}
