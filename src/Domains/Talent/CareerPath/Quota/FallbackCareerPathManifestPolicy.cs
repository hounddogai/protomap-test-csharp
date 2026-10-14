namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback career path manifest policy.</summary>
public static class FallbackCareerPathManifestPolicy
{
    /// <summary>Puts a fallback career path manifest code in its canonical FCPM- form.</summary>
    public static string NormalizeFallbackCareerPathManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FCPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FCPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackCareerPathManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double FallbackCareerPathManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
