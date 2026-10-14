namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility rule policy.</summary>
public static class MobilityRulePolicy
{
    public static double MobilityRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a mobility rule score falls in.</summary>
    public static string ClassifyMobilityRule(double score)
    {
        if (score < 35.2)
        {
            return "watch";
        }
        if (score < 50.2)
        {
            return "elevated";
        }
        return "low";
    }

    public static int MobilityRuleSpan(int start, int end)
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
        return span / 86400;
    }
}
