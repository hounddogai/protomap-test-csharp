namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor metric policy.</summary>
public static class VendorMetricPolicy
{
    public static int VendorMetricSpan(int start, int end)
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
        return span / 60;
    }

    public static double VendorMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
