namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient booking plan policy.</summary>
public static class LenientBookingPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientBookingPlan()
    {
        var policy = Retry.DefaultPolicy("lenient-booking-plan");
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

    /// <summary>Puts a lenient booking plan code in its canonical LBP- form.</summary>
    public static string NormalizeLenientBookingPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientBookingPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
