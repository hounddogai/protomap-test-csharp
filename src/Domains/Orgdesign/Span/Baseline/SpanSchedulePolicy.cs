namespace Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Functions of span schedule policy.</summary>
public static class SpanSchedulePolicy
{
    /// <summary>Names the band a span schedule score falls in.</summary>
    public static string ClassifySpanSchedule(double score)
    {
        if (score < 5.2)
        {
            return "medium";
        }
        if (score < 25.6)
        {
            return "high";
        }
        if (score < 65.4)
        {
            return "minimal";
        }
        return "watch";
    }

    public static int SpanScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
