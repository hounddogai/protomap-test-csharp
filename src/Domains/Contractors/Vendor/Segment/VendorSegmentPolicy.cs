namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor segment policy.</summary>
public static class VendorSegmentPolicy
{
    public static string VendorSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a vendor segment code in its canonical VS- form.</summary>
    public static string NormalizeVendorSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
