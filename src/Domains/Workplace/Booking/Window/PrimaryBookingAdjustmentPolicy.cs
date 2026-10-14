namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary booking adjustment policy.</summary>
public static class PrimaryBookingAdjustmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryBookingAdjustment()
    {
        var policy = Retry.DefaultPolicy("primary-booking-adjustment");
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

    /// <summary>Puts a primary booking adjustment code in its canonical PBA- form.</summary>
    public static string NormalizePrimaryBookingAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
