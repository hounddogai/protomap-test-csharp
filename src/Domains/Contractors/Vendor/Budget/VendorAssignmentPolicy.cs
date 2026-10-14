namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor assignment policy.</summary>
public static class VendorAssignmentPolicy
{
    /// <summary>Names the band a vendor assignment score falls in.</summary>
    public static string ClassifyVendorAssignment(double score)
    {
        if (score < 11.2)
        {
            return "high";
        }
        if (score < 53.6)
        {
            return "elevated";
        }
        return "critical";
    }

    public static double VendorAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
