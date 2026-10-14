namespace Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Functions of pending access badge request policy.</summary>
public static class PendingAccessBadgeRequestPolicy
{
    /// <summary>Names the band a pending access badge request score falls in.</summary>
    public static string ClassifyPendingAccessBadgeRequest(double score)
    {
        if (score < 3.9)
        {
            return "elevated";
        }
        if (score < 16.5)
        {
            return "ok";
        }
        if (score < 39.7)
        {
            return "low";
        }
        if (score < 78.2)
        {
            return "watch";
        }
        return "medium";
    }

    public static int PendingAccessBadgeRequestSpan(int start, int end)
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
