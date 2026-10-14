namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback vendor manifest policy.</summary>
public static class FallbackVendorManifestPolicy
{
    public static int FallbackVendorManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a fallback vendor manifest code in its canonical FVM- form.</summary>
    public static string NormalizeFallbackVendorManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FVM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FVM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackVendorManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeFallbackVendorManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
