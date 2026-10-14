namespace Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Functions of fallback recognition digest policy.</summary>
public static class FallbackRecognitionDigestPolicy
{
    /// <summary>Names the band a fallback recognition digest score falls in.</summary>
    public static string ClassifyFallbackRecognitionDigest(double score)
    {
        if (score < 44.8)
        {
            return "elevated";
        }
        if (score < 87.8)
        {
            return "ok";
        }
        return "watch";
    }

    public static int FallbackRecognitionDigestSpan(int start, int end)
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
