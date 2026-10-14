namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Functions of scheduled deduction estimate from budget.</summary>
public static class ScheduledDeductionEstimateFromBudget
{
    public static Dictionary<string, double> SummarizeWeightedGarnishmentBudgetsForEstimate(List<WeightedGarnishmentBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ScheduledDeductionEstimate> DeriveScheduledDeductionEstimatesFromBudget(List<WeightedGarnishmentBudget> items)
    {
        var outValue = new List<ScheduledDeductionEstimate>();
        foreach (var item in items)
        {
            var derived = ScheduledDeductionEstimate.NewScheduledDeductionEstimate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeScheduledDeductionEstimate(ScheduledDeductionEstimate item)
    {
        return GarnishmentSlotPolicy.GarnishmentSlotBucket(item.MeasuredValue());
    }
}
