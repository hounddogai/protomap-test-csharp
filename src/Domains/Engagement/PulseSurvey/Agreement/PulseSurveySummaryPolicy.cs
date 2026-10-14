namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey summary policy.</summary>
public static class PulseSurveySummaryPolicy
{
    public static string PulseSurveySummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int PulseSurveySummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a pulse survey summary score falls in.</summary>
    public static string ClassifyPulseSurveySummary(double score)
    {
        if (score < 30.9)
        {
            return "watch";
        }
        if (score < 44.7)
        {
            return "minimal";
        }
        if (score < 88.8)
        {
            return "ok";
        }
        if (score < 97.5)
        {
            return "high";
        }
        return "medium";
    }

    public static List<string> MergePulseSurveySummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
