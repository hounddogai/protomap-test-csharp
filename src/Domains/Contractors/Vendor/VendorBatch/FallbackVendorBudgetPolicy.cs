namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback vendor budget policy.</summary>
public static class FallbackVendorBudgetPolicy
{
    /// <summary>Names the band a fallback vendor budget score falls in.</summary>
    public static string ClassifyFallbackVendorBudget(double score)
    {
        if (score < 45.2)
        {
            return "critical";
        }
        if (score < 97.7)
        {
            return "high";
        }
        return "elevated";
    }

    public static double FallbackVendorBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
