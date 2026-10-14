namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default desk score policy.</summary>
public static class DefaultDeskScorePolicy
{
    /// <summary>Names the band a default desk score score falls in.</summary>
    public static string ClassifyDefaultDeskScore(double score)
    {
        if (score < 39.4)
        {
            return "ok";
        }
        if (score < 44.8)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Puts a default desk score code in its canonical DDS- form.</summary>
    public static string NormalizeDefaultDeskScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DefaultDeskScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
