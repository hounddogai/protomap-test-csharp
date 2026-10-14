namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of vendor profile policy.</summary>
public static class VendorProfilePolicy
{
    public static double VendorProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a vendor profile score falls in.</summary>
    public static string ClassifyVendorProfile(double score)
    {
        if (score < 4.9)
        {
            return "elevated";
        }
        if (score < 69.9)
        {
            return "watch";
        }
        if (score < 77.2)
        {
            return "minimal";
        }
        if (score < 81.3)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVendorProfile()
    {
        var policy = Retry.DefaultPolicy("vendor-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
