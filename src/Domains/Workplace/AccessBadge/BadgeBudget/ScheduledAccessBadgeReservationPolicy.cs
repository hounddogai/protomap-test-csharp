namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled access badge reservation policy.</summary>
public static class ScheduledAccessBadgeReservationPolicy
{
    /// <summary>Names the band a scheduled access badge reservation score falls in.</summary>
    public static string ClassifyScheduledAccessBadgeReservation(double score)
    {
        if (score < 40.4)
        {
            return "elevated";
        }
        if (score < 79.7)
        {
            return "watch";
        }
        if (score < 87.1)
        {
            return "critical";
        }
        if (score < 97.6)
        {
            return "high";
        }
        return "ok";
    }

    public static double ScheduledAccessBadgeReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a scheduled access badge reservation code in its canonical SABR- form.</summary>
    public static string NormalizeScheduledAccessBadgeReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SABR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SABR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledAccessBadgeReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
