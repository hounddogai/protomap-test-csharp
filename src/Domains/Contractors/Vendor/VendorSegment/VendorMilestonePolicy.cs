namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor milestone policy.</summary>
public static class VendorMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 19427.0, in order.</summary>
    public static int VendorMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19427.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a vendor milestone score falls in.</summary>
    public static string ClassifyVendorMilestone(double score)
    {
        if (score < 10.6)
        {
            return "elevated";
        }
        if (score < 74.1)
        {
            return "minimal";
        }
        if (score < 90.0)
        {
            return "watch";
        }
        return "critical";
    }

    public static double VendorMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
