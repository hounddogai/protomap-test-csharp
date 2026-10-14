namespace Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Functions of strict vendor request policy.</summary>
public static class StrictVendorRequestPolicy
{
    public static int StrictVendorRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a strict vendor request code in its canonical SVR- form.</summary>
    public static string NormalizeStrictVendorRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SVR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SVR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
