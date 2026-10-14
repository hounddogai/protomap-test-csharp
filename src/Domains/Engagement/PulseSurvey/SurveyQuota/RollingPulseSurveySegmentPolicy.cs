namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Functions of rolling pulse survey segment policy.</summary>
public static class RollingPulseSurveySegmentPolicy
{
    /// <summary>Names the band a rolling pulse survey segment score falls in.</summary>
    public static string ClassifyRollingPulseSurveySegment(double score)
    {
        if (score < 4.2)
        {
            return "elevated";
        }
        if (score < 16.0)
        {
            return "high";
        }
        if (score < 19.3)
        {
            return "critical";
        }
        if (score < 57.6)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Puts a rolling pulse survey segment code in its canonical RPSS- form.</summary>
    public static string NormalizeRollingPulseSurveySegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
