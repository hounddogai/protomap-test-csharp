namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Functions of adjustment summary policy.</summary>
public static class AdjustmentSummaryPolicy
{
    /// <summary>Names the band a adjustment summary score falls in.</summary>
    public static string ClassifyAdjustmentSummary(double score)
    {
        if (score < 6.8)
        {
            return "watch";
        }
        if (score < 39.5)
        {
            return "high";
        }
        if (score < 50.7)
        {
            return "low";
        }
        if (score < 95.9)
        {
            return "medium";
        }
        return "ok";
    }

    public static int AdjustmentSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
