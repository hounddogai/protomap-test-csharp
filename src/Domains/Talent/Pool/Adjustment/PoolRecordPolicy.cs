namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool record policy.</summary>
public static class PoolRecordPolicy
{
    public static List<string> MergePoolRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pool record code in its canonical PR- form.</summary>
    public static string NormalizePoolRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
