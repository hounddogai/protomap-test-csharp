namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback training due segment policy.</summary>
public static class FallbackTrainingDueSegmentPolicy
{
    /// <summary>Names the band a fallback training due segment score falls in.</summary>
    public static string ClassifyFallbackTrainingDueSegment(double score)
    {
        if (score < 74.7)
        {
            return "ok";
        }
        if (score < 81.6)
        {
            return "critical";
        }
        if (score < 89.6)
        {
            return "watch";
        }
        if (score < 94.3)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a fallback training due segment code in its canonical FTDS- form.</summary>
    public static string NormalizeFallbackTrainingDueSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FTDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FTDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFallbackTrainingDueSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
