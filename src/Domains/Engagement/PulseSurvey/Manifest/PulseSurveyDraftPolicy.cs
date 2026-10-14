namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey draft policy.</summary>
public static class PulseSurveyDraftPolicy
{
    public static int PulseSurveyDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergePulseSurveyDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pulse survey draft score falls in.</summary>
    public static string ClassifyPulseSurveyDraft(double score)
    {
        if (score < 4.3)
        {
            return "low";
        }
        if (score < 19.7)
        {
            return "minimal";
        }
        if (score < 41.8)
        {
            return "watch";
        }
        if (score < 97.6)
        {
            return "ok";
        }
        return "high";
    }

    public static string PulseSurveyDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
