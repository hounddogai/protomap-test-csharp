namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Functions of weighted payslip budget from allocation.</summary>
public static class WeightedPayslipBudgetFromAllocation
{
    public static Dictionary<string, double> SummarizePrimaryGarnishmentAllocationsForBudget(List<PrimaryGarnishmentAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<WeightedPayslipBudget> DeriveWeightedPayslipBudgetsFromAllocation(List<PrimaryGarnishmentAllocation> items)
    {
        var outValue = new List<WeightedPayslipBudget>();
        foreach (var item in items)
        {
            var derived = WeightedPayslipBudget.NewWeightedPayslipBudget(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchWeightedPayslipBudget(WeightedPayslipBudget item)
    {
        return LegacyGarnishmentSlotPolicy.LegacyGarnishmentSlotWeight(9, true);
    }
}
