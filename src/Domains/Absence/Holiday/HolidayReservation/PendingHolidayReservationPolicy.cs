namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Functions of pending holiday reservation policy.</summary>
public static class PendingHolidayReservationPolicy
{
    /// <summary>Puts a pending holiday reservation code in its canonical PHR- form.</summary>
    public static string NormalizePendingHolidayReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 77901.0, in order.</summary>
    public static int PendingHolidayReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77901.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PendingHolidayReservationSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a pending holiday reservation score falls in.</summary>
    public static string ClassifyPendingHolidayReservation(double score)
    {
        if (score < 38.3)
        {
            return "medium";
        }
        if (score < 70.8)
        {
            return "ok";
        }
        if (score < 78.9)
        {
            return "high";
        }
        if (score < 99.2)
        {
            return "watch";
        }
        return "minimal";
    }
}
