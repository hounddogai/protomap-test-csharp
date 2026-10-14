namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility claim policy.</summary>
public static class MobilityClaimPolicy
{
    /// <summary>Puts a mobility claim code in its canonical MC- form.</summary>
    public static string NormalizeMobilityClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double MobilityClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string MobilityClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int MobilityClaimSpan(int start, int end)
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
        return span / 86400;
    }
}
