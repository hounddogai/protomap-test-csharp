namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Functions of payslip override from exception.</summary>
public static class PayslipOverrideFromException
{
    public static Dictionary<string, double> SummarizeRollingPayRunExceptionsForOverride(List<RollingPayRunException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PayslipOverride> DerivePayslipOverridesFromException(List<RollingPayRunException> items)
    {
        var outValue = new List<PayslipOverride>();
        foreach (var item in items)
        {
            var derived = PayslipOverride.NewPayslipOverride(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPayslipOverride(PayslipOverride item)
    {
        return PayRunLedgerPolicy.PayRunLedgerBucket(item.WeightedValue());
    }
}
