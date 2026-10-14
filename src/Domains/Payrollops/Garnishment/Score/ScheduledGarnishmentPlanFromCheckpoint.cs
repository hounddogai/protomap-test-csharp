namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Functions of scheduled garnishment plan from checkpoint.</summary>
public static class ScheduledGarnishmentPlanFromCheckpoint
{
    public static Dictionary<string, double> SummarizeGlobalPayslipCheckpointsForPlan(List<GlobalPayslipCheckpoint> items)
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

    public static List<ScheduledGarnishmentPlan> DeriveScheduledGarnishmentPlansFromCheckpoint(List<GlobalPayslipCheckpoint> items)
    {
        var outValue = new List<ScheduledGarnishmentPlan>();
        foreach (var item in items)
        {
            var derived = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateScheduledGarnishmentPlan(ScheduledGarnishmentPlan item)
    {
        return CappedPayslipBatchPolicy.CappedPayslipBatchBucket(item.WeightedValue());
    }
}
