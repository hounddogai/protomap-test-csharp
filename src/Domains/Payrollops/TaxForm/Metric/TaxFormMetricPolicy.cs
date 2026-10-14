namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form metric policy.</summary>
public static class TaxFormMetricPolicy
{
    public static double TaxFormMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.03;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tax form metric score falls in.</summary>
    public static string ClassifyTaxFormMetric(double score)
    {
        if (score < 12.8)
        {
            return "ok";
        }
        if (score < 38.9)
        {
            return "critical";
        }
        if (score < 96.9)
        {
            return "minimal";
        }
        return "medium";
    }

    public static int TaxFormMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
