namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary warranty profile policy.</summary>
public static class PrimaryWarrantyProfilePolicy
{
    public static int PrimaryWarrantyProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a primary warranty profile code in its canonical PWP- form.</summary>
    public static string NormalizePrimaryWarrantyProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PWP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PWP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePrimaryWarrantyProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
