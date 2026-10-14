namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending booking digest policy.</summary>
public static class PendingBookingDigestPolicy
{
    /// <summary>Names the band a pending booking digest score falls in.</summary>
    public static string ClassifyPendingBookingDigest(double score)
    {
        if (score < 39.0)
        {
            return "minimal";
        }
        if (score < 43.1)
        {
            return "critical";
        }
        if (score < 48.8)
        {
            return "ok";
        }
        if (score < 90.0)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 91591.0, in order.</summary>
    public static int PendingBookingDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91591.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingBookingDigest()
    {
        var policy = Retry.DefaultPolicy("pending-booking-digest");
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

    public static double PendingBookingDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
