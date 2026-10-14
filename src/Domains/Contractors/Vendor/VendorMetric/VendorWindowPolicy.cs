namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of vendor window policy.</summary>
public static class VendorWindowPolicy
{
    /// <summary>Puts a vendor window code in its canonical VW- form.</summary>
    public static string NormalizeVendorWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 20183.0, in order.</summary>
    public static int VendorWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20183.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double VendorWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int VendorWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
