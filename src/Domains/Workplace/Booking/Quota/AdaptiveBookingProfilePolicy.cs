namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive booking profile policy.</summary>
public static class AdaptiveBookingProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97068.0, in order.</summary>
    public static int AdaptiveBookingProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97068.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBookingProfile()
    {
        var policy = Retry.DefaultPolicy("adaptive-booking-profile");
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
