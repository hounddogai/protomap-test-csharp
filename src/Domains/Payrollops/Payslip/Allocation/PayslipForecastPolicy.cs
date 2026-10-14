namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of payslip forecast policy.</summary>
public static class PayslipForecastPolicy
{
    public static List<string> MergePayslipForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PayslipForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a payslip forecast score falls in.</summary>
    public static string ClassifyPayslipForecast(double score)
    {
        if (score < 5.8)
        {
            return "low";
        }
        if (score < 47.1)
        {
            return "high";
        }
        if (score < 90.2)
        {
            return "minimal";
        }
        if (score < 92.7)
        {
            return "medium";
        }
        return "watch";
    }

    public static string PayslipForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
