namespace Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Functions of vendor baseline policy.</summary>
public static class VendorBaselinePolicy
{
    public static int VendorBaselineSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a vendor baseline code in its canonical VB- form.</summary>
    public static string NormalizeVendorBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
