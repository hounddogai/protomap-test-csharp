namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Functions of adaptive garnishment budget from budget.</summary>
public static class AdaptiveGarnishmentBudgetFromBudget
{
    public static Dictionary<string, double> SummarizeBatchedDeductionBudgetsForBudget(List<BatchedDeductionBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<AdaptiveGarnishmentBudget> DeriveAdaptiveGarnishmentBudgetsFromBudget(List<BatchedDeductionBudget> items)
    {
        var outValue = new List<AdaptiveGarnishmentBudget>();
        foreach (var item in items)
        {
            var derived = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceAdaptiveGarnishmentBudget(AdaptiveGarnishmentBudget item)
    {
        return PrimaryDeductionOverridePolicy.NormalizePrimaryDeductionOverrideCode(item.Code);
    }
}
