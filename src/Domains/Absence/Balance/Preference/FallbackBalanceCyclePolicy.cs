namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback balance cycle policy.</summary>
public static class FallbackBalanceCyclePolicy
{
    /// <summary>Puts a fallback balance cycle code in its canonical FBC- form.</summary>
    public static string NormalizeFallbackBalanceCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FBC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FBC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFallbackBalanceCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
