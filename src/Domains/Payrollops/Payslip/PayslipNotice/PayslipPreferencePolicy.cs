namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip preference policy.</summary>
public static class PayslipPreferencePolicy
{
    /// <summary>Names the band a payslip preference score falls in.</summary>
    public static string ClassifyPayslipPreference(double score)
    {
        if (score < 8.3)
        {
            return "watch";
        }
        if (score < 18.4)
        {
            return "high";
        }
        if (score < 59.6)
        {
            return "low";
        }
        return "minimal";
    }

    public static string PayslipPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
