namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict pulse survey override policy.</summary>
public static class StrictPulseSurveyOverridePolicy
{
    public static List<string> MergeStrictPulseSurveyOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict pulse survey override score falls in.</summary>
    public static string ClassifyStrictPulseSurveyOverride(double score)
    {
        if (score < 35.1)
        {
            return "minimal";
        }
        if (score < 62.6)
        {
            return "high";
        }
        if (score < 90.7)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPulseSurveyOverride()
    {
        var policy = Retry.DefaultPolicy("strict-pulse-survey-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string StrictPulseSurveyOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
