namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of vendor manifest policy.</summary>
public static class VendorManifestPolicy
{
    public static double VendorManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVendorManifest()
    {
        var policy = Retry.DefaultPolicy("vendor-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a vendor manifest score falls in.</summary>
    public static string ClassifyVendorManifest(double score)
    {
        if (score < 8.1)
        {
            return "watch";
        }
        if (score < 22.8)
        {
            return "low";
        }
        if (score < 64.0)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 41180.0, in order.</summary>
    public static int VendorManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41180.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
