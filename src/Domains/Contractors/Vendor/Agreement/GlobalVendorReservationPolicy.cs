namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global vendor reservation policy.</summary>
public static class GlobalVendorReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalVendorReservation()
    {
        var policy = Retry.DefaultPolicy("global-vendor-reservation");
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

    public static string GlobalVendorReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double GlobalVendorReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a global vendor reservation score falls in.</summary>
    public static string ClassifyGlobalVendorReservation(double score)
    {
        if (score < 15.5)
        {
            return "high";
        }
        if (score < 54.1)
        {
            return "medium";
        }
        if (score < 95.9)
        {
            return "minimal";
        }
        return "ok";
    }
}
