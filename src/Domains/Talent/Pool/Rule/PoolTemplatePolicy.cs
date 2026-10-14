namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool template policy.</summary>
public static class PoolTemplatePolicy
{
    /// <summary>Puts a pool template code in its canonical PT- form.</summary>
    public static string NormalizePoolTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePoolTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
