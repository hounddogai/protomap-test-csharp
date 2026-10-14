namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient booking exception policy.</summary>
public static class LenientBookingExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientBookingException()
    {
        var policy = Retry.DefaultPolicy("lenient-booking-exception");
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

    /// <summary>Counts how many amounts fit in a budget of 28580.0, in order.</summary>
    public static int LenientBookingExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28580.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
