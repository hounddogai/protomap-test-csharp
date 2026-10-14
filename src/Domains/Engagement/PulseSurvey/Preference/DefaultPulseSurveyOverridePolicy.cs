namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default pulse survey override policy.</summary>
public static class DefaultPulseSurveyOverridePolicy
{
    public static double DefaultPulseSurveyOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeDefaultPulseSurveyOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a default pulse survey override score falls in.</summary>
    public static string ClassifyDefaultPulseSurveyOverride(double score)
    {
        if (score < 40.9)
        {
            return "low";
        }
        if (score < 52.8)
        {
            return "elevated";
        }
        if (score < 76.6)
        {
            return "watch";
        }
        if (score < 78.7)
        {
            return "ok";
        }
        return "high";
    }
}
