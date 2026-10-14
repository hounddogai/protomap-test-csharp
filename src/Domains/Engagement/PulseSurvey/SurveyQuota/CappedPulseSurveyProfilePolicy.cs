namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped pulse survey profile policy.</summary>
public static class CappedPulseSurveyProfilePolicy
{
    public static string CappedPulseSurveyProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double CappedPulseSurveyProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CappedPulseSurveyProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a capped pulse survey profile code in its canonical CPSP- form.</summary>
    public static string NormalizeCappedPulseSurveyProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
