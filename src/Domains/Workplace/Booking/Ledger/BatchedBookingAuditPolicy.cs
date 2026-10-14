namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched booking audit policy.</summary>
public static class BatchedBookingAuditPolicy
{
    /// <summary>Puts a batched booking audit code in its canonical BBA- form.</summary>
    public static string NormalizeBatchedBookingAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BatchedBookingAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double BatchedBookingAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
