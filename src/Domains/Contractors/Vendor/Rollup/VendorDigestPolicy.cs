namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor digest policy.</summary>
public static class VendorDigestPolicy
{
    public static string VendorDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 75748.0, in order.</summary>
    public static int VendorDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75748.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
