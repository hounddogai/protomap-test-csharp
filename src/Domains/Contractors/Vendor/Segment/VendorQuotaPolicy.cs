namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of vendor quota policy.</summary>
public static class VendorQuotaPolicy
{
    /// <summary>Names the band a vendor quota score falls in.</summary>
    public static string ClassifyVendorQuota(double score)
    {
        if (score < 33.9)
        {
            return "watch";
        }
        if (score < 42.9)
        {
            return "critical";
        }
        if (score < 67.6)
        {
            return "high";
        }
        if (score < 91.3)
        {
            return "ok";
        }
        return "low";
    }

    public static string VendorQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVendorQuota()
    {
        var policy = Retry.DefaultPolicy("vendor-quota");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
