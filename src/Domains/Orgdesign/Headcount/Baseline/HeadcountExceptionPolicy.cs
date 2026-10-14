namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount exception policy.</summary>
public static class HeadcountExceptionPolicy
{
    public static List<string> MergeHeadcountExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a headcount exception code in its canonical HE- form.</summary>
    public static string NormalizeHeadcountExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
