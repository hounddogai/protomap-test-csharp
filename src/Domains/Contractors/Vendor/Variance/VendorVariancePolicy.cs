namespace Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Functions of vendor variance policy.</summary>
public static class VendorVariancePolicy
{
    public static int VendorVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a vendor variance score falls in.</summary>
    public static string ClassifyVendorVariance(double score)
    {
        if (score < 6.0)
        {
            return "elevated";
        }
        if (score < 79.3)
        {
            return "low";
        }
        return "high";
    }
}
