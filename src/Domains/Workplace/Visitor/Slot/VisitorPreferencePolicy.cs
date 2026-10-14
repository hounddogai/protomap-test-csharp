namespace Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Functions of visitor preference policy.</summary>
public static class VisitorPreferencePolicy
{
    /// <summary>Names the band a visitor preference score falls in.</summary>
    public static string ClassifyVisitorPreference(double score)
    {
        if (score < 27.3)
        {
            return "critical";
        }
        if (score < 52.3)
        {
            return "watch";
        }
        if (score < 69.0)
        {
            return "ok";
        }
        if (score < 71.2)
        {
            return "medium";
        }
        return "high";
    }

    public static int VisitorPreferenceSpan(int start, int end)
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
}
