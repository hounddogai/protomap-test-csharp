namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient span rollup policy.</summary>
public static class LenientSpanRollupPolicy
{
    /// <summary>Names the band a lenient span rollup score falls in.</summary>
    public static string ClassifyLenientSpanRollup(double score)
    {
        if (score < 40.3)
        {
            return "low";
        }
        if (score < 60.3)
        {
            return "medium";
        }
        if (score < 95.0)
        {
            return "high";
        }
        return "critical";
    }

    public static int LenientSpanRollupSpan(int start, int end)
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

    public static string LenientSpanRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
