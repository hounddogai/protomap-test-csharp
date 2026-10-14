namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey rollup policy.</summary>
public static class PulseSurveyRollupPolicy
{
    /// <summary>Puts a pulse survey rollup code in its canonical PSR- form.</summary>
    public static string NormalizePulseSurveyRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PulseSurveyRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pulse survey rollup score falls in.</summary>
    public static string ClassifyPulseSurveyRollup(double score)
    {
        if (score < 8.4)
        {
            return "medium";
        }
        if (score < 18.6)
        {
            return "watch";
        }
        if (score < 73.4)
        {
            return "critical";
        }
        if (score < 73.9)
        {
            return "minimal";
        }
        return "high";
    }

    public static string PulseSurveyRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
