namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted skill profile budget policy.</summary>
public static class WeightedSkillProfileBudgetPolicy
{
    public static double WeightedSkillProfileBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WeightedSkillProfileBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int WeightedSkillProfileBudgetSpan(int start, int end)
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
