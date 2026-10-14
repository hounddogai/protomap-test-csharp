namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor draft policy.</summary>
public static class VendorDraftPolicy
{
    public static int VendorDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double VendorDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a vendor draft code in its canonical VD- form.</summary>
    public static string NormalizeVendorDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
