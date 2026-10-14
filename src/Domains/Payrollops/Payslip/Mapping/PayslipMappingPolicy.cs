namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip mapping policy.</summary>
public static class PayslipMappingPolicy
{
    public static double PayslipMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a payslip mapping score falls in.</summary>
    public static string ClassifyPayslipMapping(double score)
    {
        if (score < 1.2)
        {
            return "medium";
        }
        if (score < 68.6)
        {
            return "high";
        }
        if (score < 84.8)
        {
            return "minimal";
        }
        return "ok";
    }

    public static string PayslipMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
