namespace Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Functions of retro pay estimate policy.</summary>
public static class RetroPayEstimatePolicy
{
    public static int RetroPayEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a retro pay estimate score falls in.</summary>
    public static string ClassifyRetroPayEstimate(double score)
    {
        if (score < 48.1)
        {
            return "minimal";
        }
        if (score < 55.2)
        {
            return "high";
        }
        if (score < 68.6)
        {
            return "critical";
        }
        if (score < 79.1)
        {
            return "elevated";
        }
        return "low";
    }
}
