namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking target policy.</summary>
public static class BookingTargetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingTarget()
    {
        var policy = Retry.DefaultPolicy("booking-target");
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

    /// <summary>Counts how many amounts fit in a budget of 16627.0, in order.</summary>
    public static int BookingTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16627.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a booking target code in its canonical BT- form.</summary>
    public static string NormalizeBookingTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
