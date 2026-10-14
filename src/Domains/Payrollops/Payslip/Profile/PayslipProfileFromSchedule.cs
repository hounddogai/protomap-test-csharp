namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Functions of payslip profile from schedule.</summary>
public static class PayslipProfileFromSchedule
{
    public static Dictionary<string, double> SummarizeGarnishmentSchedulesForProfile(List<GarnishmentSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PayslipProfile> DerivePayslipProfilesFromSchedule(List<GarnishmentSchedule> items)
    {
        var outValue = new List<PayslipProfile>();
        foreach (var item in items)
        {
            var derived = PayslipProfile.NewPayslipProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePayslipProfile(PayslipProfile item)
    {
        return FallbackGarnishmentRecordPolicy.ClassifyFallbackGarnishmentRecord(item.WeightedExposure());
    }
}
