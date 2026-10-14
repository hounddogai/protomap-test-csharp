namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Functions of cost model allocation from budget.</summary>
public static class CostModelAllocationFromBudget
{
    public static Dictionary<string, double> SummarizePendingHeadcountBudgetsForAllocation(List<PendingHeadcountBudget> items)
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

    public static List<CostModelAllocation> DeriveCostModelAllocationsFromBudget(List<PendingHeadcountBudget> items)
    {
        var outValue = new List<CostModelAllocation>();
        foreach (var item in items)
        {
            var derived = CostModelAllocation.NewCostModelAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCostModelAllocation(CostModelAllocation item)
    {
        return PendingHeadcountBudgetPolicy.ClassifyPendingHeadcountBudget(item.MeasuredValue());
    }
}
