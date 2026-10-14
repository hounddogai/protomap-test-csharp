namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position reservation policy.</summary>
public static class PositionReservationPolicy
{
    public static string PositionReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double PositionReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a position reservation score falls in.</summary>
    public static string ClassifyPositionReservation(double score)
    {
        if (score < 2.8)
        {
            return "medium";
        }
        if (score < 75.5)
        {
            return "minimal";
        }
        if (score < 99.1)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Puts a position reservation code in its canonical PR- form.</summary>
    public static string NormalizePositionReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
