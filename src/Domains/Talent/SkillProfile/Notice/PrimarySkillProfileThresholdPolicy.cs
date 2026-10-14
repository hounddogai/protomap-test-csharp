namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary skill profile threshold policy.</summary>
public static class PrimarySkillProfileThresholdPolicy
{
    public static string PrimarySkillProfileThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergePrimarySkillProfileThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PrimarySkillProfileThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary skill profile threshold score falls in.</summary>
    public static string ClassifyPrimarySkillProfileThreshold(double score)
    {
        if (score < 46.9)
        {
            return "low";
        }
        if (score < 47.3)
        {
            return "ok";
        }
        if (score < 66.0)
        {
            return "medium";
        }
        if (score < 70.3)
        {
            return "watch";
        }
        return "minimal";
    }
}
