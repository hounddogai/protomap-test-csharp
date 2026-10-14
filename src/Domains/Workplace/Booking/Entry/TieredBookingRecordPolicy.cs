namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered booking record policy.</summary>
public static class TieredBookingRecordPolicy
{
    /// <summary>Puts a tiered booking record code in its canonical TBR- form.</summary>
    public static string NormalizeTieredBookingRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredBookingRecord()
    {
        var policy = Retry.DefaultPolicy("tiered-booking-record");
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
}
