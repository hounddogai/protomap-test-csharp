namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped skill profile forecast policy.</summary>
public static class CappedSkillProfileForecastPolicy
{
    public static List<string> MergeCappedSkillProfileForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a capped skill profile forecast score falls in.</summary>
    public static string ClassifyCappedSkillProfileForecast(double score)
    {
        if (score < 37.5)
        {
            return "ok";
        }
        if (score < 44.2)
        {
            return "medium";
        }
        if (score < 62.4)
        {
            return "low";
        }
        return "high";
    }
}
