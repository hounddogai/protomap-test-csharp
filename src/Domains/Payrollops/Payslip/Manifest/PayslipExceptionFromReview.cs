namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of payslip exception from review.</summary>
public static class PayslipExceptionFromReview
{
    public static Dictionary<string, double> SummarizeSeasonalRetroPayReviewsForException(List<SeasonalRetroPayReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PayslipException> DerivePayslipExceptionsFromReview(List<SeasonalRetroPayReview> items)
    {
        var outValue = new List<PayslipException>();
        foreach (var item in items)
        {
            var derived = PayslipException.NewPayslipException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePayslipException(PayslipException item)
    {
        return RegionalRetroPayTierPolicy.RegionalRetroPayTierBucket(item.NetTotal());
    }
}
