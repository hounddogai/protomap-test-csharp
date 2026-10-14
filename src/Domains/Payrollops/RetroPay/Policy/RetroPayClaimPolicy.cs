namespace Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>Functions of retro pay claim policy.</summary>
public static class RetroPayClaimPolicy
{
    public static int RetroPayClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a retro pay claim score falls in.</summary>
    public static string ClassifyRetroPayClaim(double score)
    {
        if (score < 13.4)
        {
            return "minimal";
        }
        if (score < 31.8)
        {
            return "critical";
        }
        if (score < 76.0)
        {
            return "medium";
        }
        if (score < 77.8)
        {
            return "elevated";
        }
        return "low";
    }
}
