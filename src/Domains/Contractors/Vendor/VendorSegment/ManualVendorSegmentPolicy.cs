namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual vendor segment policy.</summary>
public static class ManualVendorSegmentPolicy
{
    public static double ManualVendorSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a manual vendor segment score falls in.</summary>
    public static string ClassifyManualVendorSegment(double score)
    {
        if (score < 42.0)
        {
            return "critical";
        }
        if (score < 47.8)
        {
            return "minimal";
        }
        if (score < 93.3)
        {
            return "ok";
        }
        if (score < 94.8)
        {
            return "medium";
        }
        return "low";
    }
}
