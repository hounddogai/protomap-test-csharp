namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk digest policy.</summary>
public static class DeskDigestPolicy
{
    /// <summary>Names the band a desk digest score falls in.</summary>
    public static string ClassifyDeskDigest(double score)
    {
        if (score < 6.8)
        {
            return "critical";
        }
        if (score < 24.1)
        {
            return "elevated";
        }
        return "ok";
    }

    public static List<string> MergeDeskDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a desk digest code in its canonical DD- form.</summary>
    public static string NormalizeDeskDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
