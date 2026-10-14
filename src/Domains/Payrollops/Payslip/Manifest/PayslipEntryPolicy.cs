namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip entry policy.</summary>
public static class PayslipEntryPolicy
{
    /// <summary>Names the band a payslip entry score falls in.</summary>
    public static string ClassifyPayslipEntry(double score)
    {
        if (score < 54.1)
        {
            return "watch";
        }
        if (score < 63.8)
        {
            return "critical";
        }
        if (score < 70.4)
        {
            return "medium";
        }
        if (score < 90.8)
        {
            return "minimal";
        }
        return "ok";
    }

    public static string PayslipEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
