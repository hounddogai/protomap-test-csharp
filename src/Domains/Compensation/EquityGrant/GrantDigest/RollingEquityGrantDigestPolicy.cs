namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling equity grant digest policy.</summary>
public static class RollingEquityGrantDigestPolicy
{
    /// <summary>Puts a rolling equity grant digest code in its canonical REGD- form.</summary>
    public static string NormalizeRollingEquityGrantDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("REGD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"REGD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingEquityGrantDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
