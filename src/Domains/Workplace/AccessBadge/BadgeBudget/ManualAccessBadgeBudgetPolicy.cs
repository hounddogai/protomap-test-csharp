namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Functions of manual access badge budget policy.</summary>
public static class ManualAccessBadgeBudgetPolicy
{
    public static int ManualAccessBadgeBudgetSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a manual access badge budget score falls in.</summary>
    public static string ClassifyManualAccessBadgeBudget(double score)
    {
        if (score < 68.4)
        {
            return "medium";
        }
        if (score < 87.4)
        {
            return "ok";
        }
        return "minimal";
    }
}
