namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor threshold policy.</summary>
public static class VendorThresholdPolicy
{
    /// <summary>Puts a vendor threshold code in its canonical VT- form.</summary>
    public static string NormalizeVendorThresholdCode(string code)
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

    public static double VendorThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int VendorThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
