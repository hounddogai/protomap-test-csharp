namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled engagement notice policy.</summary>
public static class ScheduledEngagementNoticePolicy
{
    public static int ScheduledEngagementNoticeSpan(int start, int end)
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

    /// <summary>Puts a scheduled engagement notice code in its canonical SEN- form.</summary>
    public static string NormalizeScheduledEngagementNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledEngagementNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
