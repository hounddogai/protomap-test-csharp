namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Functions of pay run bundle from reservation.</summary>
public static class PayRunBundleFromReservation
{
    public static Dictionary<string, double> SummarizeDeductionReservationsForBundle(List<DeductionReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PayRunBundle> DerivePayRunBundlesFromReservation(List<DeductionReservation> items)
    {
        var outValue = new List<PayRunBundle>();
        foreach (var item in items)
        {
            var derived = PayRunBundle.NewPayRunBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitPayRunBundle(PayRunBundle item)
    {
        return DeductionBudgetPolicy.DeductionBudgetWeight(6, true);
    }
}
