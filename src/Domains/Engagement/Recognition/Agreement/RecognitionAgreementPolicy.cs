namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition agreement policy.</summary>
public static class RecognitionAgreementPolicy
{
    /// <summary>Names the band a recognition agreement score falls in.</summary>
    public static string ClassifyRecognitionAgreement(double score)
    {
        if (score < 31.1)
        {
            return "high";
        }
        if (score < 75.7)
        {
            return "critical";
        }
        if (score < 85.3)
        {
            return "minimal";
        }
        return "watch";
    }

    public static List<string> MergeRecognitionAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int RecognitionAgreementSpan(int start, int end)
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

    /// <summary>Puts a recognition agreement code in its canonical RA- form.</summary>
    public static string NormalizeRecognitionAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
