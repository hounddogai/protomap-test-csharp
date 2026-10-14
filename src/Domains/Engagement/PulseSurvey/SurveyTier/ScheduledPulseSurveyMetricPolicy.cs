namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled pulse survey metric policy.</summary>
public static class ScheduledPulseSurveyMetricPolicy
{
    /// <summary>Puts a scheduled pulse survey metric code in its canonical SPSM- form.</summary>
    public static string NormalizeScheduledPulseSurveyMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScheduledPulseSurveyMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
