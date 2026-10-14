namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal pulse survey audit policy.</summary>
public static class SeasonalPulseSurveyAuditPolicy
{
    public static string SeasonalPulseSurveyAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a seasonal pulse survey audit score falls in.</summary>
    public static string ClassifySeasonalPulseSurveyAudit(double score)
    {
        if (score < 8.4)
        {
            return "elevated";
        }
        if (score < 76.0)
        {
            return "ok";
        }
        if (score < 82.2)
        {
            return "low";
        }
        if (score < 92.8)
        {
            return "medium";
        }
        return "high";
    }

    public static List<string> MergeSeasonalPulseSurveyAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
