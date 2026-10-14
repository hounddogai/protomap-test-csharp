namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Functions of batched deduction budget from window.</summary>
public static class BatchedDeductionBudgetFromWindow
{
    public static Dictionary<string, double> SummarizeRegionalPayRunWindowsForBudget(List<RegionalPayRunWindow> items)
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

    public static List<BatchedDeductionBudget> DeriveBatchedDeductionBudgetsFromWindow(List<RegionalPayRunWindow> items)
    {
        var outValue = new List<BatchedDeductionBudget>();
        foreach (var item in items)
        {
            var derived = BatchedDeductionBudget.NewBatchedDeductionBudget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectBatchedDeductionBudget(BatchedDeductionBudget item)
    {
        return LenientPayRunAgreementPolicy.ClassifyLenientPayRunAgreement(item.ProjectedTotal());
    }
}
