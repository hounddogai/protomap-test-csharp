namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Functions of pending tax form override from exception.</summary>
public static class PendingTaxFormOverrideFromException
{
    public static Dictionary<string, double> SummarizeLenientPayslipExceptionsForOverride(List<LenientPayslipException> items)
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

    public static List<PendingTaxFormOverride> DerivePendingTaxFormOverridesFromException(List<LenientPayslipException> items)
    {
        var outValue = new List<PendingTaxFormOverride>();
        foreach (var item in items)
        {
            var derived = PendingTaxFormOverride.NewPendingTaxFormOverride(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchPendingTaxFormOverride(PendingTaxFormOverride item)
    {
        return LenientPayslipExceptionPolicy.LenientPayslipExceptionWeight(8, true);
    }
}
