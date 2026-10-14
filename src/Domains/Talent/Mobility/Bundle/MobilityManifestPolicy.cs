namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility manifest policy.</summary>
public static class MobilityManifestPolicy
{
    public static string MobilityManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a mobility manifest code in its canonical MM- form.</summary>
    public static string NormalizeMobilityManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
