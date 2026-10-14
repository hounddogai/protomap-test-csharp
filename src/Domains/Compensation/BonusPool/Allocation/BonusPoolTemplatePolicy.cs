namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool template policy.</summary>
public static class BonusPoolTemplatePolicy
{
    /// <summary>Names the band a bonus pool template score falls in.</summary>
    public static string ClassifyBonusPoolTemplate(double score)
    {
        if (score < 6.8)
        {
            return "medium";
        }
        if (score < 7.5)
        {
            return "minimal";
        }
        if (score < 57.4)
        {
            return "high";
        }
        return "elevated";
    }

    public static double BonusPoolTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
