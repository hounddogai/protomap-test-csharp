namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Functions of pending deduction exception from rule.</summary>
public static class PendingDeductionExceptionFromRule
{
    public static Dictionary<string, double> SummarizeCappedPayslipRulesForException(List<CappedPayslipRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PendingDeductionException> DerivePendingDeductionExceptionsFromRule(List<CappedPayslipRule> items)
    {
        var outValue = new List<PendingDeductionException>();
        foreach (var item in items)
        {
            var derived = PendingDeductionException.NewPendingDeductionException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPendingDeductionException(PendingDeductionException item)
    {
        return CappedPayslipTargetPolicy.CappedPayslipTargetBucket(item.NetExposure());
    }
}
