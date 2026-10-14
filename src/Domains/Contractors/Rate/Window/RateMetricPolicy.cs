namespace Acme.Workforce.Contractors.Rate.Window;

/// <summary>Functions of rate metric policy.</summary>
public static class RateMetricPolicy
{
    /// <summary>Names the band a rate metric score falls in.</summary>
    public static string ClassifyRateMetric(double score)
    {
        if (score < 24.9)
        {
            return "watch";
        }
        if (score < 78.4)
        {
            return "medium";
        }
        if (score < 89.1)
        {
            return "minimal";
        }
        return "critical";
    }

    public static int RateMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}
