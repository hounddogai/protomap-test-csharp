namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling nomination threshold policy.</summary>
public static class RollingNominationThresholdPolicy
{
    /// <summary>Names the band a rolling nomination threshold score falls in.</summary>
    public static string ClassifyRollingNominationThreshold(double score)
    {
        if (score < 33.1)
        {
            return "critical";
        }
        if (score < 48.8)
        {
            return "low";
        }
        if (score < 81.1)
        {
            return "ok";
        }
        if (score < 94.8)
        {
            return "high";
        }
        return "medium";
    }

    public static string RollingNominationThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RollingNominationThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling nomination threshold code in its canonical RNT- form.</summary>
    public static string NormalizeRollingNominationThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RNT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RNT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
