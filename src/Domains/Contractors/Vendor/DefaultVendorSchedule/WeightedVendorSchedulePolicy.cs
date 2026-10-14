namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted vendor schedule policy.</summary>
public static class WeightedVendorSchedulePolicy
{
    /// <summary>Names the band a weighted vendor schedule score falls in.</summary>
    public static string ClassifyWeightedVendorSchedule(double score)
    {
        if (score < 2.2)
        {
            return "elevated";
        }
        if (score < 44.7)
        {
            return "ok";
        }
        return "watch";
    }

    public static double WeightedVendorScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
