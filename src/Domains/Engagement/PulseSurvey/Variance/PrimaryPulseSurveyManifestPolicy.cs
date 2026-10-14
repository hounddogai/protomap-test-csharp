namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary pulse survey manifest policy.</summary>
public static class PrimaryPulseSurveyManifestPolicy
{
    public static List<string> MergePrimaryPulseSurveyManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a primary pulse survey manifest code in its canonical PPSM- form.</summary>
    public static string NormalizePrimaryPulseSurveyManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PPSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PPSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
