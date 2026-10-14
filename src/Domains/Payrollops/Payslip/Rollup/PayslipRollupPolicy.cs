namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip rollup policy.</summary>
public static class PayslipRollupPolicy
{
    public static string PayslipRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a payslip rollup score falls in.</summary>
    public static string ClassifyPayslipRollup(double score)
    {
        if (score < 7.5)
        {
            return "critical";
        }
        if (score < 50.8)
        {
            return "elevated";
        }
        if (score < 67.5)
        {
            return "low";
        }
        if (score < 69.5)
        {
            return "watch";
        }
        return "minimal";
    }
}
