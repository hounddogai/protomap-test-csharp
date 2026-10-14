namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient offboarding forecast policy.</summary>
public static class LenientOffboardingForecastPolicy
{
    /// <summary>Puts a lenient offboarding forecast code in its canonical LOF- form.</summary>
    public static string NormalizeLenientOffboardingForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a lenient offboarding forecast score falls in.</summary>
    public static string ClassifyLenientOffboardingForecast(double score)
    {
        if (score < 25.3)
        {
            return "medium";
        }
        if (score < 81.7)
        {
            return "watch";
        }
        if (score < 88.7)
        {
            return "critical";
        }
        if (score < 92.5)
        {
            return "ok";
        }
        return "minimal";
    }

    public static string LenientOffboardingForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
