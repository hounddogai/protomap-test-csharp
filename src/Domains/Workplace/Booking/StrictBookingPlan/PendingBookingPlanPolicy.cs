namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending booking plan policy.</summary>
public static class PendingBookingPlanPolicy
{
    public static string PendingBookingPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a pending booking plan score falls in.</summary>
    public static string ClassifyPendingBookingPlan(double score)
    {
        if (score < 8.7)
        {
            return "medium";
        }
        if (score < 30.7)
        {
            return "high";
        }
        if (score < 65.5)
        {
            return "minimal";
        }
        if (score < 91.5)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a pending booking plan code in its canonical PBP- form.</summary>
    public static string NormalizePendingBookingPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
