namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default skill profile policy policy.</summary>
public static class DefaultSkillProfilePolicyPolicy
{
    public static double DefaultSkillProfilePolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a default skill profile policy score falls in.</summary>
    public static string ClassifyDefaultSkillProfilePolicy(double score)
    {
        if (score < 3.7)
        {
            return "watch";
        }
        if (score < 39.0)
        {
            return "medium";
        }
        if (score < 81.7)
        {
            return "minimal";
        }
        if (score < 96.0)
        {
            return "high";
        }
        return "low";
    }
}
