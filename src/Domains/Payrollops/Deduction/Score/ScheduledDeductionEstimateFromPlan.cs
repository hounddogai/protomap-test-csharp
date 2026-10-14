namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Functions of scheduled deduction estimate from plan.</summary>
public static class ScheduledDeductionEstimateFromPlan
{
    public static Dictionary<string, double> SummarizeRegionalPayRunPlansForEstimate(List<RegionalPayRunPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ScheduledDeductionEstimate> DeriveScheduledDeductionEstimatesFromPlan(List<RegionalPayRunPlan> items)
    {
        var outValue = new List<ScheduledDeductionEstimate>();
        foreach (var item in items)
        {
            var derived = ScheduledDeductionEstimate.NewScheduledDeductionEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanScheduledDeductionEstimate(ScheduledDeductionEstimate item)
    {
        return RegionalPayRunPlanPolicy.RegionalPayRunPlanBucket(item.MeasuredValue());
    }
}
