namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Functions of pending retro pay review from threshold.</summary>
public static class PendingRetroPayReviewFromThreshold
{
    public static Dictionary<string, double> SummarizePayslipThresholdsForReview(List<PayslipThreshold> items)
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

    public static List<PendingRetroPayReview> DerivePendingRetroPayReviewsFromThreshold(List<PayslipThreshold> items)
    {
        var outValue = new List<PendingRetroPayReview>();
        foreach (var item in items)
        {
            var derived = PendingRetroPayReview.NewPendingRetroPayReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcilePendingRetroPayReview(PendingRetroPayReview item)
    {
        return PayslipSummaryPolicy.PayslipSummaryWeight(6, true);
    }
}
