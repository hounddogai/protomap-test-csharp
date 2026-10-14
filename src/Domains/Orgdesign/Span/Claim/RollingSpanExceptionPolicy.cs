namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling span exception policy.</summary>
public static class RollingSpanExceptionPolicy
{
    /// <summary>Puts a rolling span exception code in its canonical RSE- form.</summary>
    public static string NormalizeRollingSpanExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingSpanExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
