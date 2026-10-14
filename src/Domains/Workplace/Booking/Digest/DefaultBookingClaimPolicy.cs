namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default booking claim policy.</summary>
public static class DefaultBookingClaimPolicy
{
    /// <summary>Names the band a default booking claim score falls in.</summary>
    public static string ClassifyDefaultBookingClaim(double score)
    {
        if (score < 67.3)
        {
            return "watch";
        }
        if (score < 69.0)
        {
            return "high";
        }
        if (score < 80.6)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultBookingClaim()
    {
        var policy = Retry.DefaultPolicy("default-booking-claim");
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

    public static string DefaultBookingClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
