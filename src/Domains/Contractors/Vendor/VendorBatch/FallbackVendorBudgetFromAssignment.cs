namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Functions of fallback vendor budget from assignment.</summary>
public static class FallbackVendorBudgetFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedStatementOfWorkAssignmentsForBudget(List<CappedStatementOfWorkAssignment> items)
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

    public static List<FallbackVendorBudget> DeriveFallbackVendorBudgetsFromAssignment(List<CappedStatementOfWorkAssignment> items)
    {
        var outValue = new List<FallbackVendorBudget>();
        foreach (var item in items)
        {
            var derived = FallbackVendorBudget.NewFallbackVendorBudget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileFallbackVendorBudget(FallbackVendorBudget item)
    {
        return StatementOfWorkBatchPolicy.ClassifyStatementOfWorkBatch(item.AdjustedVolume());
    }
}
