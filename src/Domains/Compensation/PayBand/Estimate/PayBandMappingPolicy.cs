namespace Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Functions of pay band mapping policy.</summary>
public static class PayBandMappingPolicy
{
    /// <summary>Names the band a pay band mapping score falls in.</summary>
    public static string ClassifyPayBandMapping(double score)
    {
        if (score < 2.2)
        {
            return "watch";
        }
        if (score < 12.6)
        {
            return "low";
        }
        return "medium";
    }

    public static int PayBandMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
