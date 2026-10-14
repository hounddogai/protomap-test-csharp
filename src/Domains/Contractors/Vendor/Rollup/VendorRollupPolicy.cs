namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Functions of vendor rollup policy.</summary>
public static class VendorRollupPolicy
{
    /// <summary>Names the band a vendor rollup score falls in.</summary>
    public static string ClassifyVendorRollup(double score)
    {
        if (score < 20.8)
        {
            return "elevated";
        }
        if (score < 29.1)
        {
            return "ok";
        }
        if (score < 33.0)
        {
            return "critical";
        }
        if (score < 84.7)
        {
            return "low";
        }
        return "watch";
    }

    public static int VendorRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a vendor rollup code in its canonical VR- form.</summary>
    public static string NormalizeVendorRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
