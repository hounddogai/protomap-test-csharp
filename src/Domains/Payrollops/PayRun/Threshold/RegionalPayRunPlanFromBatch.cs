namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Functions of regional pay run plan from batch.</summary>
public static class RegionalPayRunPlanFromBatch
{
    public static Dictionary<string, double> SummarizeSeasonalPayRunBatchesForPlan(List<SeasonalPayRunBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<RegionalPayRunPlan> DeriveRegionalPayRunPlansFromBatch(List<SeasonalPayRunBatch> items)
    {
        var outValue = new List<RegionalPayRunPlan>();
        foreach (var item in items)
        {
            var derived = RegionalPayRunPlan.NewRegionalPayRunPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRegionalPayRunPlan(RegionalPayRunPlan item)
    {
        return ScheduledPayRunAuditPolicy.ScheduledPayRunAuditBucket(item.AdjustedVolume());
    }
}
