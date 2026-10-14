namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional headcount budget policy.</summary>
public static class RegionalHeadcountBudgetPolicy
{
    public static double RegionalHeadcountBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a regional headcount budget score falls in.</summary>
    public static string ClassifyRegionalHeadcountBudget(double score)
    {
        if (score < 19.6)
        {
            return "elevated";
        }
        if (score < 30.8)
        {
            return "watch";
        }
        if (score < 31.4)
        {
            return "low";
        }
        if (score < 37.3)
        {
            return "ok";
        }
        return "medium";
    }

    public static string RegionalHeadcountBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
