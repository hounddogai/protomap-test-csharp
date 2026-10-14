namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg entry policy.</summary>
public static class ReorgEntryPolicy
{
    public static List<string> MergeReorgEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a reorg entry code in its canonical RE- form.</summary>
    public static string NormalizeReorgEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
