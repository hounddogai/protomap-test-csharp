namespace Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Functions of vendor tier policy.</summary>
public static class VendorTierPolicy
{
    /// <summary>Puts a vendor tier code in its canonical VT- form.</summary>
    public static string NormalizeVendorTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int VendorTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
