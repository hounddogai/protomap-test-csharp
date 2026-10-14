namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback bonus pool profile policy.</summary>
public static class FallbackBonusPoolProfilePolicy
{
    /// <summary>Names the band a fallback bonus pool profile score falls in.</summary>
    public static string ClassifyFallbackBonusPoolProfile(double score)
    {
        if (score < 37.0)
        {
            return "elevated";
        }
        if (score < 69.0)
        {
            return "watch";
        }
        if (score < 72.3)
        {
            return "low";
        }
        return "minimal";
    }

    public static double FallbackBonusPoolProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
