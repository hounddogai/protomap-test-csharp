namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Functions of lenient career path rule policy.</summary>
public static class LenientCareerPathRulePolicy
{
    public static int LenientCareerPathRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a lenient career path rule score falls in.</summary>
    public static string ClassifyLenientCareerPathRule(double score)
    {
        if (score < 34.4)
        {
            return "critical";
        }
        if (score < 77.7)
        {
            return "watch";
        }
        return "medium";
    }
}
