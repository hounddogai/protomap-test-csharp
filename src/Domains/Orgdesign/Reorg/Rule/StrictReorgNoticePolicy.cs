namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict reorg notice policy.</summary>
public static class StrictReorgNoticePolicy
{
    /// <summary>Puts a strict reorg notice code in its canonical SRN- form.</summary>
    public static string NormalizeStrictReorgNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeStrictReorgNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
