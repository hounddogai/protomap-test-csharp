namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Functions of payslip manifest from forecast.</summary>
public static class PayslipManifestFromForecast
{
    public static Dictionary<string, double> SummarizeCappedRetroPayForecastsForManifest(List<CappedRetroPayForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<PayslipManifest> DerivePayslipManifestsFromForecast(List<CappedRetroPayForecast> items)
    {
        var outValue = new List<PayslipManifest>();
        foreach (var item in items)
        {
            var derived = PayslipManifest.NewPayslipManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePayslipManifest(PayslipManifest item)
    {
        return RetroPayAgreementPolicy.RetroPayAgreementWeight(6, true);
    }
}
