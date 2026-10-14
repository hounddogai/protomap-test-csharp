namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy recognition cycle policy.</summary>
public static class LegacyRecognitionCyclePolicy
{
    public static List<string> MergeLegacyRecognitionCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy recognition cycle code in its canonical LRC- form.</summary>
    public static string NormalizeLegacyRecognitionCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
