namespace Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Functions of engagement preference policy.</summary>
public static class EngagementPreferencePolicy
{
    public static int EngagementPreferenceSpan(int start, int end)
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

    /// <summary>Names the band a engagement preference score falls in.</summary>
    public static string ClassifyEngagementPreference(double score)
    {
        if (score < 85.0)
        {
            return "low";
        }
        if (score < 97.4)
        {
            return "medium";
        }
        return "minimal";
    }
}
