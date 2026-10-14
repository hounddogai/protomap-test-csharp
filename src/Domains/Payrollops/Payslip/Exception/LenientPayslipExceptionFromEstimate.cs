namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Functions of lenient payslip exception from estimate.</summary>
public static class LenientPayslipExceptionFromEstimate
{
    public static Dictionary<string, double> SummarizeLenientGarnishmentEstimatesForException(List<LenientGarnishmentEstimate> items)
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

    public static List<LenientPayslipException> DeriveLenientPayslipExceptionsFromEstimate(List<LenientGarnishmentEstimate> items)
    {
        var outValue = new List<LenientPayslipException>();
        foreach (var item in items)
        {
            var derived = LenientPayslipException.NewLenientPayslipException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLenientPayslipException(LenientPayslipException item)
    {
        return AdaptiveGarnishmentPlanPolicy.AdaptiveGarnishmentPlanBucket(item.ProjectedExposure());
    }
}
