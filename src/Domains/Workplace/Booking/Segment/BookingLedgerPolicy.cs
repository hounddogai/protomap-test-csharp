namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking ledger policy.</summary>
public static class BookingLedgerPolicy
{
    public static int BookingLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingLedger()
    {
        var policy = Retry.DefaultPolicy("booking-ledger");
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

    public static string BookingLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a booking ledger code in its canonical BL- form.</summary>
    public static string NormalizeBookingLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
