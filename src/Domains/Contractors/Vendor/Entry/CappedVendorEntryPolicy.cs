namespace Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of capped vendor entry policy.</summary>
public static class CappedVendorEntryPolicy
{
    /// <summary>Puts a capped vendor entry code in its canonical CVE- form.</summary>
    public static string NormalizeCappedVendorEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CVE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CVE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped vendor entry score falls in.</summary>
    public static string ClassifyCappedVendorEntry(double score)
    {
        if (score < 24.8)
        {
            return "watch";
        }
        if (score < 49.5)
        {
            return "low";
        }
        if (score < 49.9)
        {
            return "medium";
        }
        if (score < 79.1)
        {
            return "elevated";
        }
        return "minimal";
    }
}
