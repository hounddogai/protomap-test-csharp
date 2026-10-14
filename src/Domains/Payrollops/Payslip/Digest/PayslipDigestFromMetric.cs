namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Functions of payslip digest from metric.</summary>
public static class PayslipDigestFromMetric
{
    public static Dictionary<string, double> SummarizeGarnishmentMetricsForDigest(List<GarnishmentMetric> items)
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

    public static List<PayslipDigest> DerivePayslipDigestsFromMetric(List<GarnishmentMetric> items)
    {
        var outValue = new List<PayslipDigest>();
        foreach (var item in items)
        {
            var derived = PayslipDigest.NewPayslipDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPayslipDigest(PayslipDigest item)
    {
        return GarnishmentMetricPolicy.GarnishmentMetricWeight(5, true);
    }
}
