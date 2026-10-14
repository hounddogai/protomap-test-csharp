namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Functions of tiered timesheet budget from bundle.</summary>
public static class TieredTimesheetBudgetFromBundle
{
    public static Dictionary<string, double> SummarizeRollingRateBundlesForBudget(List<RollingRateBundle> items)
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

    public static List<TieredTimesheetBudget> DeriveTieredTimesheetBudgetsFromBundle(List<RollingRateBundle> items)
    {
        var outValue = new List<TieredTimesheetBudget>();
        foreach (var item in items)
        {
            var derived = TieredTimesheetBudget.NewTieredTimesheetBudget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredTimesheetBudget(TieredTimesheetBudget item)
    {
        return FallbackRateCyclePolicy.NormalizeFallbackRateCycleCode(item.Code);
    }
}
