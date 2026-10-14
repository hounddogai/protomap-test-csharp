namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Functions of default payslip budget from plan.</summary>
public static class DefaultPayslipBudgetFromPlan
{
    public static Dictionary<string, double> SummarizeRegionalPayRunPlansForBudget(List<RegionalPayRunPlan> items)
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

    public static List<DefaultPayslipBudget> DeriveDefaultPayslipBudgetsFromPlan(List<RegionalPayRunPlan> items)
    {
        var outValue = new List<DefaultPayslipBudget>();
        foreach (var item in items)
        {
            var derived = DefaultPayslipBudget.NewDefaultPayslipBudget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultPayslipBudget(DefaultPayslipBudget item)
    {
        return RegionalPayRunPlanPolicy.RegionalPayRunPlanBucket(item.EffectiveExposure());
    }
}
