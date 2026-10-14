namespace Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of legacy vendor batch policy.</summary>
public static class LegacyVendorBatchPolicy
{
    /// <summary>Names the band a legacy vendor batch score falls in.</summary>
    public static string ClassifyLegacyVendorBatch(double score)
    {
        if (score < 31.1)
        {
            return "low";
        }
        if (score < 43.3)
        {
            return "minimal";
        }
        if (score < 50.1)
        {
            return "critical";
        }
        if (score < 97.6)
        {
            return "elevated";
        }
        return "high";
    }

    public static int LegacyVendorBatchSpan(int start, int end)
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
        return span / 3600;
    }
}
