namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual pulse survey forecast policy.</summary>
public static class ManualPulseSurveyForecastPolicy
{
    /// <summary>Puts a manual pulse survey forecast code in its canonical MPSF- form.</summary>
    public static string NormalizeManualPulseSurveyForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MPSF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MPSF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ManualPulseSurveyForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
