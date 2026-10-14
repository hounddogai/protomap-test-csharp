namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey review policy.</summary>
public static class PulseSurveyReviewPolicy
{
    /// <summary>Puts a pulse survey review code in its canonical PSR- form.</summary>
    public static string NormalizePulseSurveyReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PulseSurveyReviewSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergePulseSurveyReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
