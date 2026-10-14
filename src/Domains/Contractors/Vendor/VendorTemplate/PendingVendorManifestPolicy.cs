namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending vendor manifest policy.</summary>
public static class PendingVendorManifestPolicy
{
    public static double PendingVendorManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PendingVendorManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a pending vendor manifest score falls in.</summary>
    public static string ClassifyPendingVendorManifest(double score)
    {
        if (score < 3.9)
        {
            return "low";
        }
        if (score < 34.7)
        {
            return "minimal";
        }
        if (score < 47.3)
        {
            return "high";
        }
        if (score < 89.8)
        {
            return "critical";
        }
        return "ok";
    }
}
