namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Functions of legacy payslip digest from audit.</summary>
public static class LegacyPayslipDigestFromAudit
{
    public static Dictionary<string, double> SummarizePrimaryRetroPayAuditsForDigest(List<PrimaryRetroPayAudit> items)
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

    public static List<LegacyPayslipDigest> DeriveLegacyPayslipDigestsFromAudit(List<PrimaryRetroPayAudit> items)
    {
        var outValue = new List<LegacyPayslipDigest>();
        foreach (var item in items)
        {
            var derived = LegacyPayslipDigest.NewLegacyPayslipDigest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLegacyPayslipDigest(LegacyPayslipDigest item)
    {
        return RetroPayReservationPolicy.ClassifyRetroPayReservation(item.ProjectedTotal());
    }
}
