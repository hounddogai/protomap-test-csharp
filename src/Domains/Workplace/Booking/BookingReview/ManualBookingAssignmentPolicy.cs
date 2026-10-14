namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual booking assignment policy.</summary>
public static class ManualBookingAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualBookingAssignment()
    {
        var policy = Retry.DefaultPolicy("manual-booking-assignment");
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

    public static int ManualBookingAssignmentSpan(int start, int end)
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

    /// <summary>Puts a manual booking assignment code in its canonical MBA- form.</summary>
    public static string NormalizeManualBookingAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ManualBookingAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
