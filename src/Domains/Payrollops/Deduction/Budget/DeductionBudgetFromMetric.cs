namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of deduction budget from metric.</summary>
public static class DeductionBudgetFromMetric
{
    public static Dictionary<string, double> SummarizeBatchedGarnishmentMetricsForBudget(List<BatchedGarnishmentMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DeductionBudget> DeriveDeductionBudgetsFromMetric(List<BatchedGarnishmentMetric> items)
    {
        var outValue = new List<DeductionBudget>();
        foreach (var item in items)
        {
            var derived = DeductionBudget.NewDeductionBudget(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDeductionBudget(DeductionBudget item)
    {
        return BatchedGarnishmentMetricPolicy.NormalizeBatchedGarnishmentMetricCode(item.Code);
    }
}
