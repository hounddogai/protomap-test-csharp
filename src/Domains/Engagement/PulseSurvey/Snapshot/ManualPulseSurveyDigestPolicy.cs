namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual pulse survey digest policy.</summary>
public static class ManualPulseSurveyDigestPolicy
{
    public static int ManualPulseSurveyDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a manual pulse survey digest score falls in.</summary>
    public static string ClassifyManualPulseSurveyDigest(double score)
    {
        if (score < 30.6)
        {
            return "medium";
        }
        if (score < 60.8)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Puts a manual pulse survey digest code in its canonical MPSD- form.</summary>
    public static string NormalizeManualPulseSurveyDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MPSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MPSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeManualPulseSurveyDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
