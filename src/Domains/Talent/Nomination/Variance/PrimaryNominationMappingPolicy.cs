namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary nomination mapping policy.</summary>
public static class PrimaryNominationMappingPolicy
{
    /// <summary>Names the band a primary nomination mapping score falls in.</summary>
    public static string ClassifyPrimaryNominationMapping(double score)
    {
        if (score < 6.1)
        {
            return "watch";
        }
        if (score < 31.2)
        {
            return "low";
        }
        if (score < 33.2)
        {
            return "elevated";
        }
        if (score < 39.0)
        {
            return "ok";
        }
        return "critical";
    }

    public static string PrimaryNominationMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a primary nomination mapping code in its canonical PNM- form.</summary>
    public static string NormalizePrimaryNominationMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PNM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PNM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
