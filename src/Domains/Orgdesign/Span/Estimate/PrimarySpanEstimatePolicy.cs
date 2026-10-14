namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Functions of primary span estimate policy.</summary>
public static class PrimarySpanEstimatePolicy
{
    /// <summary>Names the band a primary span estimate score falls in.</summary>
    public static string ClassifyPrimarySpanEstimate(double score)
    {
        if (score < 2.5)
        {
            return "elevated";
        }
        if (score < 26.2)
        {
            return "minimal";
        }
        if (score < 87.9)
        {
            return "high";
        }
        if (score < 97.9)
        {
            return "low";
        }
        return "watch";
    }

    public static int PrimarySpanEstimateSpan(int start, int end)
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
}
