namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict pool rule policy.</summary>
public static class StrictPoolRulePolicy
{
    /// <summary>Names the band a strict pool rule score falls in.</summary>
    public static string ClassifyStrictPoolRule(double score)
    {
        if (score < 23.8)
        {
            return "medium";
        }
        if (score < 28.2)
        {
            return "ok";
        }
        if (score < 82.5)
        {
            return "elevated";
        }
        return "watch";
    }

    public static double StrictPoolRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int StrictPoolRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
