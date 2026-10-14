namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient balance checkpoint policy.</summary>
public static class LenientBalanceCheckpointPolicy
{
    public static List<string> MergeLenientBalanceCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient balance checkpoint score falls in.</summary>
    public static string ClassifyLenientBalanceCheckpoint(double score)
    {
        if (score < 14.0)
        {
            return "minimal";
        }
        if (score < 56.5)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Puts a lenient balance checkpoint code in its canonical LBC- form.</summary>
    public static string NormalizeLenientBalanceCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
