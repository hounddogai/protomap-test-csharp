namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile metric policy.</summary>
public static class SkillProfileMetricPolicy
{
    /// <summary>Names the band a skill profile metric score falls in.</summary>
    public static string ClassifySkillProfileMetric(double score)
    {
        if (score < 9.1)
        {
            return "watch";
        }
        if (score < 85.5)
        {
            return "critical";
        }
        return "minimal";
    }

    public static string SkillProfileMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
