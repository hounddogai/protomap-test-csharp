namespace Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Functions of equity grant digest policy.</summary>
public static class EquityGrantDigestPolicy
{
    public static int EquityGrantDigestSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a equity grant digest score falls in.</summary>
    public static string ClassifyEquityGrantDigest(double score)
    {
        if (score < 9.0)
        {
            return "low";
        }
        if (score < 82.8)
        {
            return "minimal";
        }
        return "critical";
    }
}
