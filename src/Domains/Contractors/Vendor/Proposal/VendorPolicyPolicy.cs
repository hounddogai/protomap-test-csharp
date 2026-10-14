namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor policy policy.</summary>
public static class VendorPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20570.0, in order.</summary>
    public static int VendorPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20570.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int VendorPolicySpan(int start, int end)
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
        return span / 86400;
    }

    public static double VendorPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
