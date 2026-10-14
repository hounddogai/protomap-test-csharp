namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Functions of manual audit schedule from variance.</summary>
public static class ManualAuditScheduleFromVariance
{
    public static Dictionary<string, double> SummarizeTieredPolicyAckVariancesForSchedule(List<TieredPolicyAckVariance> items)
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

    public static List<ManualAuditSchedule> DeriveManualAuditSchedulesFromVariance(List<TieredPolicyAckVariance> items)
    {
        var outValue = new List<ManualAuditSchedule>();
        foreach (var item in items)
        {
            var derived = ManualAuditSchedule.NewManualAuditSchedule(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignManualAuditSchedule(ManualAuditSchedule item)
    {
        return PolicyAckBaselinePolicy.PolicyAckBaselineBucket(item.ProjectedExposure());
    }
}
