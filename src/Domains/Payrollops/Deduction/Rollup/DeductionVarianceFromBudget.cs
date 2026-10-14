namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Functions of deduction variance from budget.</summary>
public static class DeductionVarianceFromBudget
{
    public static Dictionary<string, double> SummarizeDeductionBudgetsForVariance(List<DeductionBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<DeductionVariance> DeriveDeductionVariancesFromBudget(List<DeductionBudget> items)
    {
        var outValue = new List<DeductionVariance>();
        foreach (var item in items)
        {
            var derived = DeductionVariance.NewDeductionVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileDeductionVariance(DeductionVariance item)
    {
        return DeductionReservationPolicy.ClassifyDeductionReservation(item.ProjectedValue());
    }
}
