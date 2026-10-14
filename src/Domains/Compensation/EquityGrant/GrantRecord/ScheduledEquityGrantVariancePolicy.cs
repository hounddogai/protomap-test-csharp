namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

/// <summary>Functions of scheduled equity grant variance policy.</summary>
public static class ScheduledEquityGrantVariancePolicy
{
    public static int ScheduledEquityGrantVarianceSpan(int start, int end)
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

    /// <summary>Names the band a scheduled equity grant variance score falls in.</summary>
    public static string ClassifyScheduledEquityGrantVariance(double score)
    {
        if (score < 28.5)
        {
            return "watch";
        }
        if (score < 30.9)
        {
            return "critical";
        }
        if (score < 54.4)
        {
            return "elevated";
        }
        return "high";
    }
}
