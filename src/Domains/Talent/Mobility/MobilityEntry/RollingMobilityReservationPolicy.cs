namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Functions of rolling mobility reservation policy.</summary>
public static class RollingMobilityReservationPolicy
{
    /// <summary>Puts a rolling mobility reservation code in its canonical RMR- form.</summary>
    public static string NormalizeRollingMobilityReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a rolling mobility reservation score falls in.</summary>
    public static string ClassifyRollingMobilityReservation(double score)
    {
        if (score < 32.4)
        {
            return "elevated";
        }
        if (score < 76.1)
        {
            return "medium";
        }
        return "low";
    }

    public static int RollingMobilityReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
