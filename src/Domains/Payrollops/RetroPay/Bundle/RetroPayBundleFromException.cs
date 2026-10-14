namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Functions of retro pay bundle from exception.</summary>
public static class RetroPayBundleFromException
{
    public static Dictionary<string, double> SummarizeRegionalPayslipExceptionsForBundle(List<RegionalPayslipException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RetroPayBundle> DeriveRetroPayBundlesFromException(List<RegionalPayslipException> items)
    {
        var outValue = new List<RetroPayBundle>();
        foreach (var item in items)
        {
            var derived = RetroPayBundle.NewRetroPayBundle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectRetroPayBundle(RetroPayBundle item)
    {
        return AdaptivePayslipClaimPolicy.AdaptivePayslipClaimWeight(1, true);
    }
}
