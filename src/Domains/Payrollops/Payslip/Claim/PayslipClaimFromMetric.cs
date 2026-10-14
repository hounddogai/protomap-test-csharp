namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of payslip claim from metric.</summary>
public static class PayslipClaimFromMetric
{
    public static Dictionary<string, double> SummarizeBatchedGarnishmentMetricsForClaim(List<BatchedGarnishmentMetric> items)
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

    public static List<PayslipClaim> DerivePayslipClaimsFromMetric(List<BatchedGarnishmentMetric> items)
    {
        var outValue = new List<PayslipClaim>();
        foreach (var item in items)
        {
            var derived = PayslipClaim.NewPayslipClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPayslipClaim(PayslipClaim item)
    {
        return GarnishmentExceptionPolicy.NormalizeGarnishmentExceptionCode(item.Code);
    }
}
