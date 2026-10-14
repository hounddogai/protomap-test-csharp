namespace Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Functions of scheduled engagement target policy.</summary>
public static class ScheduledEngagementTargetPolicy
{
    /// <summary>Puts a scheduled engagement target code in its canonical SET- form.</summary>
    public static string NormalizeScheduledEngagementTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledEngagementTargetSpan(int start, int end)
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
        return span / 60;
    }
}
