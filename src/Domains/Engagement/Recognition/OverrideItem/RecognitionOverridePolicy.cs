namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition override policy.</summary>
public static class RecognitionOverridePolicy
{
    /// <summary>Names the band a recognition override score falls in.</summary>
    public static string ClassifyRecognitionOverride(double score)
    {
        if (score < 31.5)
        {
            return "watch";
        }
        if (score < 75.6)
        {
            return "ok";
        }
        if (score < 95.8)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a recognition override code in its canonical RO- form.</summary>
    public static string NormalizeRecognitionOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRecognitionOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string RecognitionOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
