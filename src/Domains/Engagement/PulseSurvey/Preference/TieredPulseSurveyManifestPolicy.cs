namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered pulse survey manifest policy.</summary>
public static class TieredPulseSurveyManifestPolicy
{
    public static string TieredPulseSurveyManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a tiered pulse survey manifest score falls in.</summary>
    public static string ClassifyTieredPulseSurveyManifest(double score)
    {
        if (score < 11.5)
        {
            return "critical";
        }
        if (score < 12.0)
        {
            return "ok";
        }
        if (score < 52.1)
        {
            return "minimal";
        }
        return "watch";
    }

    public static List<string> MergeTieredPulseSurveyManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPulseSurveyManifest()
    {
        var policy = Retry.DefaultPolicy("tiered-pulse-survey-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
