namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy booking variance policy.</summary>
public static class LegacyBookingVariancePolicy
{
    /// <summary>Names the band a legacy booking variance score falls in.</summary>
    public static string ClassifyLegacyBookingVariance(double score)
    {
        if (score < 11.7)
        {
            return "low";
        }
        if (score < 14.2)
        {
            return "medium";
        }
        if (score < 34.8)
        {
            return "high";
        }
        if (score < 41.8)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 30940.0, in order.</summary>
    public static int LegacyBookingVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30940.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyBookingVariance()
    {
        var policy = Retry.DefaultPolicy("legacy-booking-variance");
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
}
