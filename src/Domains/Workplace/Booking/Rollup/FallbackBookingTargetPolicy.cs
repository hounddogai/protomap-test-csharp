namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback booking target policy.</summary>
public static class FallbackBookingTargetPolicy
{
    public static string FallbackBookingTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a fallback booking target score falls in.</summary>
    public static string ClassifyFallbackBookingTarget(double score)
    {
        if (score < 36.9)
        {
            return "watch";
        }
        if (score < 46.4)
        {
            return "ok";
        }
        if (score < 48.6)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackBookingTarget()
    {
        var policy = Retry.DefaultPolicy("fallback-booking-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 87216.0, in order.</summary>
    public static int FallbackBookingTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87216.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
