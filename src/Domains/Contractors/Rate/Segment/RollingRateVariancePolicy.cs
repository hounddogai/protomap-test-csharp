namespace Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Functions of rolling rate variance policy.</summary>
public static class RollingRateVariancePolicy
{
    public static int RollingRateVarianceSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a rolling rate variance score falls in.</summary>
    public static string ClassifyRollingRateVariance(double score)
    {
        if (score < 24.6)
        {
            return "high";
        }
        if (score < 46.1)
        {
            return "critical";
        }
        return "ok";
    }
}
