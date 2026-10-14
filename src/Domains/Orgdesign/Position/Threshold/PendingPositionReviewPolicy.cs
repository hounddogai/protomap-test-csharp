namespace Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Functions of pending position review policy.</summary>
public static class PendingPositionReviewPolicy
{
    public static int PendingPositionReviewSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a pending position review score falls in.</summary>
    public static string ClassifyPendingPositionReview(double score)
    {
        if (score < 20.5)
        {
            return "high";
        }
        if (score < 35.7)
        {
            return "ok";
        }
        if (score < 88.0)
        {
            return "low";
        }
        return "elevated";
    }
}
