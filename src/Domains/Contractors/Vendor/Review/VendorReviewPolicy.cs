namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of vendor review policy.</summary>
public static class VendorReviewPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVendorReview()
    {
        var policy = Retry.DefaultPolicy("vendor-review");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a vendor review code in its canonical VR- form.</summary>
    public static string NormalizeVendorReviewCode(string code)
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
