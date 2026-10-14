namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount rule policy.</summary>
public static class HeadcountRulePolicy
{
    /// <summary>Names the band a headcount rule score falls in.</summary>
    public static string ClassifyHeadcountRule(double score)
    {
        if (score < 30.3)
        {
            return "medium";
        }
        if (score < 32.8)
        {
            return "elevated";
        }
        if (score < 73.5)
        {
            return "high";
        }
        if (score < 79.3)
        {
            return "minimal";
        }
        return "watch";
    }

    public static double HeadcountRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int HeadcountRuleSpan(int start, int end)
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
        return span / 3600;
    }
}
