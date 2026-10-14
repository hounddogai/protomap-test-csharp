namespace Acme.Workforce.Talent.Pool.Exception;

/// <summary>Functions of scheduled pool threshold policy.</summary>
public static class ScheduledPoolThresholdPolicy
{
    /// <summary>Names the band a scheduled pool threshold score falls in.</summary>
    public static string ClassifyScheduledPoolThreshold(double score)
    {
        if (score < 32.9)
        {
            return "critical";
        }
        if (score < 51.4)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Puts a scheduled pool threshold code in its canonical SPT- form.</summary>
    public static string NormalizeScheduledPoolThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
